using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Authentification;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Home;
using RemoteControlWPFClient.WpfLayer.Views.Windows;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.Events;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;

namespace RemoteControlWPFClient.WpfLayer.ViewModels
{
    public partial class MainViewModel : ViewModelBase<MainWindow>, ITransient
    {
        private readonly Client client;
        private readonly EventBus eventBus;
        private readonly CommandsRecipientService commandsRecipient;
		private readonly CurrentUserServices currentUser;
		
        [ObservableProperty]
        private UserControl userControl;

        [ObservableProperty]
        private Visibility menuVisibility;

        [ObservableProperty]
        private string userLogin;

        public MainViewModel(Client client, EventBus eventBus, CommandsRecipientService commandsRecipient,CurrentUserServices currentUser)
        {
            this.client = client;
            this.eventBus = eventBus;
            this.commandsRecipient = commandsRecipient;
			this.currentUser = currentUser;
            userLogin = this.currentUser.CurrentUser.Login;
            MenuVisibility = Visibility.Collapsed;
			eventBus?.Subscribe<ChangeUserControlEvent>(@event => ChangeUserControl(@event.UserControl));
        }

        /// <summary>
        /// Метод для смены UserControl
        /// </summary>
        /// <param name="newUserControl"></param>
        private Task ChangeUserControl(UserControl newUserControl)
        {
            if (newUserControl is AuthentifcationUC)
            {
                MenuVisibility = Visibility.Collapsed;
            }
            else
            {
                MenuVisibility = Visibility.Visible;
            }
            
            UserControl = newUserControl;
            return Task.CompletedTask;
        }

        public ICommand OpenMainControlCommand => new RelayCommand(OpenMainControl);

        private void OpenMainControl()
        {
            if (UserControl is HomeUC) return;
            HomeUC control = IoCContainer.OpenViewModel<HomeViewModel, HomeUC>();
            UserControl = control;
        }
    }
}