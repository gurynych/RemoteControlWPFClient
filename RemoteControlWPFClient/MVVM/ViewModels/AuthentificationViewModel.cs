using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm.Native;
using RemoteControlWPFClient.BusinessLogic.Services;
using RemoteControlWPFClient.BusinessLogic.Helpers;
using RemoteControlWPFClient.MVVM.Command;
using RemoteControlWPFClient.MVVM.Events;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.IoC.Services;
using RemoteControlWPFClient.MVVM.Models;
using RemoteControlWPFClient.Views.UserControls.Home;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NetworkMessage.Communicator;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using NetworkMessage.CommandsResults;
using NetworkMessage.CommandFactory;
using NetworkMessage.Intents;


namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public partial class AuthentificationViewModel : ObservableValidator, ITransient
    {
        private readonly EventBus eventBus;
        private readonly ICommandFactory factory;
        private readonly ServerAPIProviderService apiProvider;
        private readonly TcpCryptoClientCommunicator communicator;
        private CancellationTokenSource tokenSource;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Поле \"логин\" обязательно для заполненеия")]
        [RegularExpression(@"[a-zA-Z0-9]+$", ErrorMessage = "Логин должен содержать только латиские буквы и цифры")]
        [MinLength(3, ErrorMessage = "Логин должен быть от 3 до 20 символов")]
        private string regLogin;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Поле \"почта\" обязательно для заполненеия")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Почта не соответствует формату")]
        private string regEmail;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Поле \"пароль\" обязательно для заполненеия")]
        [MinLength(6, ErrorMessage = "Минимум 6 символов")]
        private string regPassword;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Поле \"почта\" обязательно для заполненеия")]
        private string authEmail;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Поле \"пароль\" обязательно для заполненеия")]
        private string authPassword;

        [ObservableProperty]
        private string actualAction;

        [ObservableProperty]
        private long receiveProcessProgress;

        [ObservableProperty]
        private long sendProcessProgress;

        public ICommand AuthorizationCommand => new AwaitableCommand(AuthorizeWithConnectToServer, obj => !HasErrors);

        public ICommand RegistrationCommand => new AwaitableCommand(RegisterWithConnectToServer, obj => !HasErrors);

        public AuthentificationViewModel(EventBus eventBus, TcpCryptoClientCommunicator client, ICommandFactory commandFactory, ServerAPIProviderService apiProvider)
        {
            this.eventBus = eventBus;
            this.communicator = client;
            factory = commandFactory;
            this.apiProvider = apiProvider;
        }

        /// <summary>
        /// Асинхронный метод для авторизации в приложении через API и установки подключения к серверу по сокету
        /// </summary>        
        private async Task AuthorizeWithConnectToServer(object obj)
        {
            try
            {
                User user = new User(AuthEmail, AuthPassword);
                tokenSource = new CancellationTokenSource(20000);
                byte[] serverPublicKey = await apiProvider.UserAuthorizationUseAPIAsync(user, tokenSource.Token);
                if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                {
                    //throw new NullReferenceException();
                    MessageBox.Show("Неверные данные","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
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
                        user.AuthToken = serverPublicKey;
                        tokenSource.Dispose();
                        await FileHelper.WriteUserToFile(user, tokenSource.Token);
                        await StartListen();
                        await eventBus.Publish(new ChangeUserControlEvent(new HomeUC()));
                        return;
                    }

                    tokenSource.TryReset();
                    repeat++;
                }

                Debug.WriteLine("Прошло 10 подключений", "Error");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Время подключения к серверу истекло", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    MessageBox.Show("Время подключения к серверу истекло", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (webEx.Response is HttpWebResponse response)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            MessageBox.Show("Неверно введенные данные", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case HttpStatusCode.NotFound:
                            MessageBox.Show("Такого пользователя не существует", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка подключения к серверу", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SEHException)
            {
                MessageBox.Show("Ошибка HWID", "Исправить", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "Error");
                MessageBox.Show("Не удалось войти", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Асинхронный метод для регистрации в приложении через API и установки подключения к серверу по сокету
        /// </summary>       
        private async Task RegisterWithConnectToServer(object obj)
        {
            try
            {
                User user = new User(RegLogin, RegEmail, RegPassword);
                tokenSource = new CancellationTokenSource(20000);
                byte[] serverPublicKey = await apiProvider.UserRegistrationUseAPIAsync(user, tokenSource.Token);
                if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                {
                    MessageBox.Show("Неверные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    //throw new NullReferenceException();
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
                        user.AuthToken = serverPublicKey;
                        tokenSource.Dispose();
                        await FileHelper.WriteUserToFile(user, tokenSource.Token);
                        await StartListen();
                        await eventBus.Publish(new ChangeUserControlEvent(new HomeUC()));
                        return;
                    }

                    tokenSource.TryReset();
                    repeat++;
                }

                Debug.WriteLine("Прошло 10 подключений", "Error");
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show("Время подключения к серверу истекло", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(ex.Message, "Error");
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    MessageBox.Show("Время подключения к серверу истекло", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (webEx.Response is HttpWebResponse response)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            MessageBox.Show("Неверно введенные данные", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case HttpStatusCode.NoContent:
                            MessageBox.Show("Такой пользователь уже сущестсвует", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                    
                }
                else
                {
                    MessageBox.Show("Ошибка подключения к серверу", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Debug.WriteLine(webEx.Message, "Error");
            }
            catch (SEHException ex)
            {
                MessageBox.Show("Ошибка HWID", "Исправить", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(ex.Message, "Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось зарегистрироваться", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(ex.Message, "Error");
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
