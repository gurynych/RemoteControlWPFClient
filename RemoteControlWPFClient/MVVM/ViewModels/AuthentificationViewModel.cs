using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm.Native;
using RemoteControlWPFClient.BusinessLogic.Helpers;
using RemoteControlWPFClient.BusinessLogic.ServerAPIProvider;
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

namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public partial class AuthentificationViewModel : ObservableValidator, ITransient
    {
        private readonly EventBus eventBus;
        private readonly Client client;

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


        public ICommand AuthorizationCommand => new AwaitableCommand(AuthorizeWithConnectToServer, obj => !HasErrors);

        public ICommand RegistrationCommand => new AwaitableCommand(RegisterWithConnectToServer, obj => !HasErrors);

        public AuthentificationViewModel(EventBus eventBus, Client client)
        {
            this.eventBus = eventBus;
            this.client = client;
        }

        /// <summary>
        /// Асинхронный метод для авторизации в приложении через API и установки подключения к серверу по сокету
        /// </summary>        
        private async Task AuthorizeWithConnectToServer(object obj)
        {
            try
            {
                User user = new User(AuthEmail, AuthPassword);
                byte[] serverPublicKey = await ServerAPIProvider.UserAuthorizationUseAPIAsync(user);
                if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                {
                    throw new NullReferenceException();
                }

                CancellationTokenSource tokenSource = new CancellationTokenSource(30000);
                if (await client.ConnectAsync(tokenSource.Token))
                {
                    client.SetExternalPublicKey(serverPublicKey);
                    await client.Handshake(tokenSource.Token);
                }
                await FileHelper.WriteUserToFile(user, tokenSource.Token);
                await eventBus.Publish(new ChangeUserControlEvent(new HomeUC()));
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
            catch (Exception)
            {
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
                byte[] serverPublicKey = await ServerAPIProvider.UserRegistrationUseAPIAsync(user);
                if (serverPublicKey == default || serverPublicKey.IsEmptyOrSingle())
                {
                    throw new NullReferenceException();
                }

                CancellationTokenSource tokenSource = new CancellationTokenSource(30000);
                if (await client.ConnectAsync(tokenSource.Token))
                {
                    client.SetExternalPublicKey(serverPublicKey);
                    await client.Handshake(tokenSource.Token);
                }

                await FileHelper.WriteUserToFile(user, tokenSource.Token);
                await eventBus.Publish(new ChangeUserControlEvent(new HomeUC()));
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Время подключения к серверу истекло", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
            catch (SEHException)
            {
                MessageBox.Show("Ошибка HWID", "Исправить", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось зарегистрироваться", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
