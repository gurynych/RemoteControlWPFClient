using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Authentification;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Home;
using RemoteControlWPFClient.WpfLayer.Views.Windows;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.Events;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Device;

namespace RemoteControlWPFClient.WpfLayer.ViewModels
{
    public partial class MainViewModel : ViewModelBase<MainWindow>, ITransient
    {
        private readonly Client client;
        private readonly EventBus eventBus;
        private readonly CommandsRecipientService commandsRecipient;
        private readonly CurrentUserServices currentUser;

        [ObservableProperty] private Control currentControl;
        [ObservableProperty] private Visibility menuVisibility;
        [ObservableProperty] private string userLogin;
        public ObservableCollection<Control> OpenedControlsHistory { get; set; }

        public MainViewModel(Client client, EventBus eventBus, CommandsRecipientService commandsRecipient,
            CurrentUserServices currentUser)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.commandsRecipient = commandsRecipient ?? throw new ArgumentNullException(nameof(commandsRecipient));
            this.currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            OpenedControlsHistory = new ObservableCollection<Control>();
            
            userLogin = currentUser.CurrentUser?.Login;
            MenuVisibility = Visibility.Collapsed;
            eventBus?.Subscribe<ChangeControlEvent>(@event => ChangeControl(@event.NewControl, @event.ClearHistory));
            OpenedControlsHistory.CollectionChanged += (_, _) => OnPropertyChanged(nameof(OpenedControlsHistory));
        }

        /// <summary>
        /// Метод для смены UserControl
        /// </summary>
        /// <param name="newControl">Новый контрол для отображения на MainWindow</param>
        /// <param name="clearHistory">Флаг, указывающий нужно ли стереть историю открытых контролов</param>
        private Task ChangeControl(Control newControl, bool clearHistory)
        {
            MenuVisibility = (newControl is AuthentifcationUC) ? Visibility.Collapsed : Visibility.Visible;
            if (clearHistory)
            {
                OpenedControlsHistory.Clear();
            }

            if (newControl is not HomeUC)
            {
                OpenedControlsHistory.Add(newControl);
            }

            UserLogin = currentUser.CurrentUser?.Login;
            CurrentControl = newControl;
            return Task.CompletedTask;
        }

        private ICommand openMainControlCommand;
        public ICommand OpenMainControlCommand => openMainControlCommand ??= new RelayCommand(OpenMainControl);

        private ICommand popOpenedControlHistoryCommand;
        public ICommand PopOpenedControlHistoryCommand => popOpenedControlHistoryCommand ??= new RelayCommand(PopOpenedControlHistory);

        private ICommand openDevicesCommand; 
        public ICommand OpenDevicesCommand => openDevicesCommand ??= new AwaitableCommand(async () =>
        {
            DevicesUC control = IoCContainer.OpenViewModel<DevicesViewModel, DevicesUC>();
            await eventBus.Publish(new ChangeControlEvent(control, false));
        });
        
        private void PopOpenedControlHistory()
        {
            if (!OpenedControlsHistory.Any()) return;
            OpenedControlsHistory.RemoveAt(OpenedControlsHistory.Count - 1);
            Control previousControl = OpenedControlsHistory.LastOrDefault();
            if (previousControl == null)
            {
                OpenMainControl();
            }
            else
            {
                CurrentControl = previousControl;
            }
        }
        
        private void OpenMainControl()
        {
            if (CurrentControl is HomeUC) return;
            OpenedControlsHistory.Clear();
            HomeUC control = IoCContainer.OpenViewModel<HomeViewModel, HomeUC>();
            CurrentControl = control;
        }
    }
}