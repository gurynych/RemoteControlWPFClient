using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm;
using Microsoft.Xaml.Behaviors.Core;
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
        private readonly ServerConectionService conectionService;

        [ObservableProperty] private Control currentControl;
        [ObservableProperty] private Visibility menuVisibility;
        [ObservableProperty] private string userLogin;
        public ObservableCollection<Control> OpenedControlsHistory { get; set; }

        public MainViewModel(Client client, EventBus eventBus, CommandsRecipientService commandsRecipient,
            CurrentUserServices currentUser, ServerConectionService conectionService)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.commandsRecipient = commandsRecipient ?? throw new ArgumentNullException(nameof(commandsRecipient));
            this.currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            this.conectionService = conectionService ?? throw new ArgumentNullException(nameof(conectionService));
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
        public ICommand OpenMainControlCommand => openMainControlCommand ??= new AwaitableCommand(OpenMainControlAsync);

        private ICommand popOpenedControlHistoryCommand;

        public ICommand PopOpenedControlHistoryCommand =>
            popOpenedControlHistoryCommand ??= new AwaitableCommand(PopOpenedControlHistoryAsync);

        private ICommand openDevicesCommand;
        public ICommand OpenDevicesCommand => openDevicesCommand ??= new AwaitableCommand(OpenDevicesAsync);

        private ICommand logOutCommad;
        public ICommand LogOutCommad => logOutCommad ??= new AwaitableCommand(LogOutAsync);

        private ICommand reconnectCommand;
        public ICommand ReconnectCommand => reconnectCommand ??= new AwaitableCommand(ReconnectAsync);
        private Task OpenDevicesAsync()
        {   
            if (CurrentControl is DevicesUC) return Task.CompletedTask;
            DevicesUC control = IoCContainer.OpenViewModel<DevicesViewModel, DevicesUC>();
            return eventBus.Publish(new ChangeControlEvent(control, false));
        }

        private Task PopOpenedControlHistoryAsync()
        {
            if (!OpenedControlsHistory.Any()) return Task.CompletedTask;
            OpenedControlsHistory.RemoveAt(OpenedControlsHistory.Count - 1);
            Control previousControl = OpenedControlsHistory.LastOrDefault();
            if (previousControl == null)
            {
                return OpenMainControlAsync();
            }
            else
            {
                CurrentControl = previousControl;
            }

            return Task.CompletedTask;
        }

        private Task OpenMainControlAsync()
        {
            if (CurrentControl is HomeUC) return Task.CompletedTask;
            HomeUC control = IoCContainer.OpenViewModel<HomeViewModel, HomeUC>();
            return eventBus.Publish(new ChangeControlEvent(control, true));
        }

        private Task LogOutAsync()
        {
            try
            {
                currentUser.Exit();
                //commandsRecipient.Dispose();
                //commandsRecipient.Stop();
            }
            catch
            {
                // ignored
            }

            AuthentifcationUC control = IoCContainer.OpenViewModel<AuthentificationViewModel, AuthentifcationUC>();
            return eventBus.Publish(new ChangeControlEvent(control, true));
        }

        private Task ReconnectAsync()
        {
            try
            {
                return conectionService.ReconnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromException(ex);
            }
        }
    }
}