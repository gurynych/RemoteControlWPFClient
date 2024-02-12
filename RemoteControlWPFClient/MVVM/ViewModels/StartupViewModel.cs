using DevExpress.Mvvm.Native;
using RemoteControlWPFClient.BusinessLogic.Helpers;
using RemoteControlWPFClient.BusinessLogic.Services;
using RemoteControlWPFClient.MVVM.Command;
using RemoteControlWPFClient.MVVM.Events;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.Models;
using RemoteControlWPFClient.Views.Windows;
using RemoteControlWPFClient.Views.UserControls.Authentification;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RemoteControlWPFClient.Views.UserControls.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.InteropServices;
using NetworkMessage.CommandFactory;
using NetworkMessage.Communicator;
using System.Diagnostics;
using NetworkMessage.CommandsResults;
using NetworkMessage.Intents;


namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public partial class StartupViewModel : ObservableValidator, ITransient
    {
        private readonly EventBus eventBus;
        private readonly ICommandFactory factory;
        private readonly ServerAPIProviderService apiProvider;
        private readonly TcpCryptoClientCommunicator communicator;
        private CancellationTokenSource tokenSource;
        private CommandsRecipientService commandsRecipientService;
		private readonly CurrentUserServices currentUser;
		[ObservableProperty]
        private string actualAction;

        [ObservableProperty]
        private long receiveProcessProgress;

        [ObservableProperty]
        private long sendProcessProgress;

        public StartupViewModel(EventBus eventBus, TcpCryptoClientCommunicator client, ICommandFactory commandFactory,
            ServerAPIProviderService apiProvider, CommandsRecipientService commandsRecipientService,CurrentUserServices currentUser)
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
                tokenSource = new CancellationTokenSource(10000);
                try
                {
                    byte[] userToken = await FileHelper.ReadUserTokenFromFile(tokenSource.Token);
                    byte[] serverPublicKey = await apiProvider.UserAuthorizationWithTokenUseAPIAsync(userToken,tokenSource.Token);

                    //доделать метод api
                    if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                    {
                        MessageBox.Show("Не удалось подключиться", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw new Exception();
                    }
                    bool connected = await communicator.ConnectAsync(ServerAPIProviderService.ServerAddress, 11000, tokenSource.Token);
                    if (!connected) return;

                    communicator.SetExternalPublicKey(serverPublicKey);
                    int repeat = 0;

                    while (repeat < 10)
                    {
                        bool success = await communicator.HandshakeAsync(token: tokenSource.Token);
                        if (success)
                        {
                            User user = await apiProvider.GetUserByToken(userToken);
							currentUser.Enter(user);
							currentUser.SetDevices(await apiProvider.GetConnectedDeviceAsync(user));
							MainWindow mainWindow = new MainWindow();
                            window.Close();
                            mainWindow.Show();
							
							await eventBus.Publish(new ChangeUserControlEvent(new HomeUC()));
                            tokenSource.Dispose();
                            return;
                        }

                        tokenSource.TryReset();
                        repeat++;
                    }

                    Debug.WriteLine("Прошло 10 подключений", "Error");
                    //else throw new Exception();
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Время подключения к серверу истекло");
                }
                catch (SEHException)
                {
                    MessageBox.Show("Ошибка HWID", "Исправить", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, "Error");
                    MainWindow mainWindow = new MainWindow();
                    window.Close();
                    mainWindow.Show();
                    await eventBus.Publish(new ChangeUserControlEvent(new AuthentifcationUC()));
                }
            }
        }

        
    }
}
