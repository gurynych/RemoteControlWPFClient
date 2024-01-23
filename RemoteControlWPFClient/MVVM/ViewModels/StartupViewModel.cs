using DevExpress.Mvvm.Native;
using RemoteControlWPFClient.BusinessLogic.Helpers;
using RemoteControlWPFClient.BusinessLogic.Services;
using RemoteControlWPFClient.MVVM.Command;
using RemoteControlWPFClient.MVVM.Events;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.IoC.Services;
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

namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public partial class StartupViewModel : ObservableValidator, ITransient
    {
        private readonly Client client;
         
        private readonly EventBus eventBus;

        public StartupViewModel(Client client,EventBus eventBus)
        {
            this.client = client;
            this.eventBus = eventBus;
        }

        public ICommand StartupWindowLoaded => new AwaitableCommand(window => ConnectToServerAsync(window));

        /// <summary>
        /// Метод для авторизации пользователя, полученного из файла через API
        /// </summary>
        private async Task ConnectToServerAsync(object obj)
        {
            if (obj is Window window)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource(10000);
                try
                {
                    User user = await FileHelper.ReadUserFromFile(tokenSource.Token);
                    ServerAPIProviderService serverAPIProviderService = new ServerAPIProviderService();
                    byte[] serverPublicKey = await serverAPIProviderService.UserAuthorizationUseAPIAsync(user);
                    if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                    {
                        //throw new NullReferenceException();
                    }

                    tokenSource = new CancellationTokenSource(10000);
                    if (await client.ConnectAsync(tokenSource.Token))
                    {
                        client.SetExternalPublicKey(serverPublicKey);
                        await client.Handshake(tokenSource.Token);

                        MainWindow mainWindow = new MainWindow();
                        window.Close();
                        mainWindow.Show();
                        await eventBus.Publish(new ChangeUserControlEvent(new HomeUC()));
                    }
                    else throw new Exception();
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Время подключения к серверу истекло");
                }
                catch (SEHException)
                {
                    MessageBox.Show("Ошибка HWID", "Исправить", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    MainWindow mainWindow = new MainWindow();
                    window.Close();
                    mainWindow.Show();
                    await eventBus.Publish(new ChangeUserControlEvent(new AuthentifcationUC()));
                }
            }
        }
    }
}
