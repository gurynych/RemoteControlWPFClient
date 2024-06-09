using DevExpress.Mvvm.Native;
using RemoteControlWPFClient.WpfLayer.Views.Windows;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Authentification;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.InteropServices;
using NetworkMessage.CommandFactory;
using NetworkMessage.Communicator;
using System.Diagnostics;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Helpers;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.Events;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;


namespace RemoteControlWPFClient.WpfLayer.ViewModels
{
    public partial class StartupViewModel : ViewModelBase<StartupWindow>, ITransient
    {
        private readonly EventBus eventBus;
        private readonly ICommandFactory factory;
        private readonly ServerAPIProvider apiProvider;
        private readonly TcpCryptoClientCommunicator communicator;
        private CancellationTokenSource tokenSource;
        private CommandsRecipientService commandsRecipientService;
        private readonly CurrentUserServices currentUser;
        
        [ObservableProperty] private string actualAction;
        [ObservableProperty] private long receiveProcessProgress;
        [ObservableProperty] private long sendProcessProgress;

        public StartupViewModel(EventBus eventBus, TcpCryptoClientCommunicator client, ICommandFactory commandFactory,
            ServerAPIProvider apiProvider, CommandsRecipientService commandsRecipientService,
            CurrentUserServices currentUser)
        {
            this.eventBus = eventBus;
            this.communicator = client;
            factory = commandFactory;
            this.apiProvider = apiProvider;
            this.commandsRecipientService = commandsRecipientService;
            this.currentUser = currentUser;
        }

        public ICommand StartupWindowLoaded => new AwaitableCommand(window => ConnectToServerAsync(window));

        /// <summary>
        /// Метод для авторизации пользователя, полученного из файла через API
        /// </summary>
        private async Task ConnectToServerAsync(object obj)
        {
            if (obj is Window window)
            {
                tokenSource = new CancellationTokenSource(120000);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                    byte[] userToken = await CredentialsHelper.ReadUserTokenFromFileAsync(tokenSource.Token);
                    byte[] serverPublicKey =
                        await apiProvider.UserAuthorizationWithTokenUseAPIAsync(userToken, tokenSource.Token);

                    //доделать метод api
                    if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                    {
                        MessageBox.Show("Не удалось подключиться", "Ошибка подключения", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    bool connected =
                        await communicator.ConnectAsync(ServerAPIProvider.ServerAddress, 11000, tokenSource.Token);
                    if (!connected) return;

                    communicator.SetExternalPublicKey(serverPublicKey);
                    int repeat = 0;

                    while (stopwatch.Elapsed.Seconds < 120)
                    {
                        bool success = await communicator.HandshakeAsync(token: tokenSource.Token);
                        if (success)
                        {
                            UserDTO userDto = await apiProvider.GetUserByToken(userToken);
                            currentUser.Enter(userDto);
                            MainWindow mainWindow = IoCContainer.OpenViewModel<MainViewModel, MainWindow>();
                            window.Close();
                            mainWindow?.Show();

                            HomeUC control = IoCContainer.OpenViewModel<HomeViewModel, HomeUC>();
                            await eventBus.Publish(new ChangeControlEvent(control, true));
                            return;
                        }

                        tokenSource.TryReset();
                        repeat++;
                    }

                    Debug.WriteLine($"Прошло {stopwatch.Elapsed.Seconds}  секунд после старта подключения", "Error");
                    //else throw new Exception();
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Время подключения к серверу истекло", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (SEHException)
                {
                    MessageBox.Show("Ошибка HWID", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, "Error");
                    MainWindow mainWindow = IoCContainer.OpenViewModel<MainViewModel, MainWindow>();
                    window.Close();
                    mainWindow.Show();

                    AuthentifcationUC control =
                        IoCContainer.OpenViewModel<AuthentificationViewModel, AuthentifcationUC>();
                    await eventBus.Publish(new ChangeControlEvent(control, false));
                }
                finally
                {
                    stopwatch.Stop();
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }
        }
    }
}