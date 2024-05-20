using CommunityToolkit.Mvvm.ComponentModel;
using NetworkMessage.CommandFactory;
using NetworkMessage.Communicator;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Home;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.Events;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Device;

namespace RemoteControlWPFClient.WpfLayer.ViewModels
{
	public partial class HomeViewModel : ValidatableViewModelBase<HomeUC>, ITransient
	{
		private readonly ICommandFactory factory;
		private readonly TcpCryptoClientCommunicator communicator;
		private readonly ServerAPIProvider apiProvider;
		private readonly CommandsRecipientService commandsRecipient;
		private readonly EventBus eventBus;
		private readonly CurrentUserServices currentUser;
		private CancellationTokenSource tokenSource;

		[ObservableProperty]
		private bool isConneted;

		[ObservableProperty]
		private string actualAction;

		[ObservableProperty]
		private int receiveProcessProgress;

		[ObservableProperty]
		private int sendProcessProgress;

		[ObservableProperty]
		private UserDTO userDto;

		public HomeViewModel(TcpCryptoClientCommunicator communicator, ICommandFactory commandFactory, 
			ServerAPIProvider apiProvider, CommandsRecipientService commandsRecipient,
			CurrentUserServices currentUser, EventBus eventBus)
		{
			this.communicator = communicator;
			this.factory = commandFactory;
			this.apiProvider = apiProvider;
			this.commandsRecipient = commandsRecipient;
			this.eventBus = eventBus;
			userDto = currentUser.CurrentUser;
			IsConneted = communicator.IsConnected;
			commandsRecipient.Start();
			tokenSource = new CancellationTokenSource();
		}

		public ICommand DisconnectCommand => new RelayCommand(() => commandsRecipient.Stop());
		
		public ICommand ExitFromAccountCommand => new RelayCommand(ExitFromAccount);

		public ICommand ReconnectCommand => new AwaitableCommand(Reconnect);

		private async Task Reconnect()
		{
			bool connected = await communicator.ConnectAsync(ServerAPIProvider.ServerAddress, 11000, tokenSource.Token);
			if (!connected) return;
			
			bool success = await communicator.HandshakeAsync(token: tokenSource.Token);
			commandsRecipient.Restart();
		}
		
		private void ExitFromAccount()
		{
			if (MessageBox.Show("Хотите выйти", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				commandsRecipient.Stop();
				currentUser.Exit();
			}
		}

		public ICommand OpenDevices => new AwaitableCommand(async () =>
		{
			DevicesUC control = IoCContainer.OpenViewModel<DevicesViewModel, DevicesUC>();
			await eventBus.Publish(new ChangeControlEvent(control, false));
		});
	}
}
