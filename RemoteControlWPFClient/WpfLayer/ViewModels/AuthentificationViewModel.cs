using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm.Native;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Home;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NetworkMessage.Communicator;
using System.Diagnostics;
using NetworkMessage.CommandFactory;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Helpers;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Authentification;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.Events;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;


namespace RemoteControlWPFClient.WpfLayer.ViewModels
{
    public partial class AuthentificationViewModel : ValidatableViewModelBase<AuthentifcationUC>, ITransient
    {
        private readonly CurrentUserServices userServices;
        private readonly EventBus eventBus;
        private readonly ICommandFactory factory;
        private readonly ServerAPIProvider apiProvider;
		private readonly CurrentUserServices currentUser;
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

        public AuthentificationViewModel(CurrentUserServices userServices, EventBus eventBus, TcpCryptoClientCommunicator client, ICommandFactory commandFactory, ServerAPIProvider apiProvider,
            CurrentUserServices currentUser)
        {
            this.userServices = userServices;
            this.eventBus = eventBus;
            this.communicator = client;
            factory = commandFactory;
            this.apiProvider = apiProvider;
			this.currentUser = currentUser;
        }

        /// <summary>
        /// Асинхронный метод для авторизации в приложении через API и установки подключения к серверу по сокету
        /// </summary>        
        private async Task AuthorizeWithConnectToServer(object obj)
        {
            try
            {
                UserDTO userDto = new UserDTO(AuthEmail, AuthPassword);
                tokenSource = new CancellationTokenSource(20000);
				byte[] userToken = await apiProvider.UserAuthorizationUseAPIAsync(userDto, tokenSource.Token);
                
				if (userToken == default || userToken.IsEmptyOrSingle())
                {
                    MessageBox.Show("Неверные данные","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                    return;
                }

                userDto = await apiProvider.GetUserByToken(userToken, tokenSource.Token);
                userServices.Enter(userDto);
				await CredentialsHelper.WriteUserTokenToFile(userToken, tokenSource.Token);

                bool connected = await communicator.ConnectAsync(ServerAPIProvider.ServerAddress, 11000, tokenSource.Token);
                if (!connected) return;

                communicator.SetExternalPublicKey(userToken);
                int repeat = 0;

                while (repeat < 10)
                {
                    bool success = await communicator.HandshakeAsync(token: tokenSource.Token);
                    if (success)
                    {
                        userDto.AuthToken = userToken;
                        currentUser.Enter(userDto);
                        HomeUC control = IoCContainer.OpenViewModel<HomeViewModel, HomeUC>();
						await eventBus.Publish(new ChangeUserControlEvent(control));
						tokenSource.Dispose();
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
            finally
            {
                tokenSource.Dispose();
            }
        }

        /// <summary>
        /// Асинхронный метод для регистрации в приложении через API и установки подключения к серверу по сокету
        /// </summary>       
        private async Task RegisterWithConnectToServer(object obj)
        {
            try
            {
                UserDTO userDto = new UserDTO(RegLogin, RegEmail, RegPassword);
                tokenSource = new CancellationTokenSource(20000);
                byte[] userToken = await apiProvider.UserRegistrationUseAPIAsync(userDto, tokenSource.Token);
                if (userToken == default || userToken.IsEmptyOrSingle())
                {
                    MessageBox.Show("Неверные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                    //throw new NullReferenceException();
                }

                userDto = await apiProvider.GetUserByToken(userToken, tokenSource.Token);
                userServices.Enter(userDto);
                await CredentialsHelper.WriteUserTokenToFile(userToken, tokenSource.Token);
                
                bool connected = await communicator.ConnectAsync(ServerAPIProvider.ServerAddress, 11000, tokenSource.Token);
                if (!connected) return;

                communicator.SetExternalPublicKey(userToken);
                int repeat = 0;

                while (repeat < 10)
                {
                    bool success = await communicator.HandshakeAsync(token: tokenSource.Token);
                    if (success)
                    {
                        HomeUC control = IoCContainer.OpenViewModel<HomeViewModel, HomeUC>();
                        await eventBus.Publish(new ChangeUserControlEvent(control));
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
            finally
            {
                tokenSource.Dispose();
            }
        }
        
    }
}
