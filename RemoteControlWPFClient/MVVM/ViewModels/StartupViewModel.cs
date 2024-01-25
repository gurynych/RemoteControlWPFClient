﻿using DevExpress.Mvvm.Native;
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

        [ObservableProperty]
        private string actualAction;

        [ObservableProperty]
        private long receiveProcessProgress;

        [ObservableProperty]
        private long sendProcessProgress;

        public StartupViewModel(EventBus eventBus, TcpCryptoClientCommunicator client, ICommandFactory commandFactory, ServerAPIProviderService apiProvider)
        {
            this.eventBus = eventBus;
            this.communicator = client;
            factory = commandFactory;
            this.apiProvider = apiProvider;
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
                    User user = await FileHelper.ReadUserFromFile(tokenSource.Token);
                    byte[] serverPublicKey = await apiProvider.UserAuthorizationUseAPIAsync(user);
                    if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                    {
                        //throw new NullReferenceException();
                    }
                    /*bool connected = await communicator.ConnectAsync(ServerAPIProviderService.ServerAddress, 11000, tokenSource.Token);
                    if (!connected) return;

                    communicator.SetExternalPublicKey(serverPublicKey);
                    int repeat = 0;

                    while (repeat < 10)
                    {
                        bool success = await communicator.HandshakeAsync(token: tokenSource.Token);
                        if (success)
                        {
                            user.AuthToken = serverPublicKey;
                            tokenSource.Dispose();
                            await FileHelper.WriteUserToFile(user, tokenSource.Token);
                            await StartListen();
                            MainWindow mainWindow = new MainWindow();
                            window.Close();
                            mainWindow.Show();
                            await eventBus.Publish(new ChangeUserControlEvent(new HomeUC()));
                            return;
                        }

                        tokenSource.TryReset();
                        repeat++;
                    }*/
                    MainWindow mainWindow = new MainWindow();
                    window.Close();
                    mainWindow.Show();
                    //ПОМЕНЯТЬ НА HOMEUC()
                    await eventBus.Publish(new ChangeUserControlEvent(new AuthentifcationUC()));
                    //Debug.WriteLine("Прошло 10 подключений", "Error");
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

        private Task StartListen()
        {
            tokenSource = new CancellationTokenSource();
            return Task.Run(async () =>
            {
                Progress<long> receiveProgress = new Progress<long>((i) => ReceiveProcessProgress = i);
                Progress<long> sendProgress = new Progress<long>((i) => SendProcessProgress = i);
                while (!tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        ActualAction = "Получение намерения\n";
                        BaseIntent intent = await communicator.ReceiveIntentAsync(receiveProgress, tokenSource.Token).ConfigureAwait(false);
                        if (intent == null)
                        {
                            continue;
                        }

                        ActualAction += $"Полученное намерение: {intent.IntentType}\n";
                        ActualAction += $"Выполнение команды\n";
                        BaseNetworkCommandResult result = await intent.CreateCommand(factory).ExecuteAsync().ConfigureAwait(false);
                        ActualAction += "Отправка результа команды\n";
                        await communicator.SendMessageAsync(result, sendProgress, tokenSource.Token).ConfigureAwait(false);
                        ActualAction += "Результат отправлен";
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            });
        }
    }
}
