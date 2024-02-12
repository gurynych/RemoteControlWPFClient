using CommunityToolkit.Mvvm.ComponentModel;
using NetworkMessage.CommandFactory;
using NetworkMessage.Communicator;
using RemoteControlWPFClient.BusinessLogic.Services;
using RemoteControlWPFClient.MVVM.Command;
using RemoteControlWPFClient.MVVM.Events;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.Models;
using RemoteControlWPFClient.Views.UserControls.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RemoteControlWPFClient.MVVM.ViewModels
{
	public partial class HomeViewModel : ObservableValidator, ITransient
	{
		private readonly ICommandFactory factory;
		private readonly TcpCryptoClientCommunicator communicator;
		private readonly ServerAPIProviderService apiProvider;
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
		private User user;

		public HomeViewModel(TcpCryptoClientCommunicator communicator, ICommandFactory commandFactory, 
			ServerAPIProviderService apiProvider, CommandsRecipientService commandsRecipient,
			CurrentUserServices currentUser, EventBus eventBus)
		{
			this.communicator = communicator;
			this.factory = commandFactory;
			this.apiProvider = apiProvider;
			this.commandsRecipient = commandsRecipient;
			this.eventBus = eventBus;
			user = currentUser.CurrentUser;
			IsConneted = communicator.IsConnected;
			commandsRecipient.Start();
			tokenSource = new CancellationTokenSource();
		}

		public ICommand DisconnectCommand => new RelayCommand(() => commandsRecipient.Stop());
		public ICommand ExitCommand => new RelayCommand(ExitFromAccount);

		public ICommand ReconnectCommand => new RelayCommand(async () =>
		{

			bool connected = await communicator.ConnectAsync(ServerAPIProviderService.ServerAddress, 11000, tokenSource.Token);
			if (!connected) return;
			bool success = await communicator.HandshakeAsync(token: tokenSource.Token);
			commandsRecipient.Restart();
		});

		private void ExitFromAccount()
		{
			if (MessageBox.Show("Хотите выйти", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				commandsRecipient.Stop();
				currentUser.Exit();
			}
		}

		private ICommand OpenDevices => new RelayCommand(async () =>
		{
			CancellationTokenSource tokenSource = new CancellationTokenSource(15000);
			await apiProvider.GetConnectedDeviceAsync(User, tokenSource.Token).ConfigureAwait(false);
			await eventBus.Publish(new ChangeUserControlEvent(new DevicesControl()));
		});
	}
}
