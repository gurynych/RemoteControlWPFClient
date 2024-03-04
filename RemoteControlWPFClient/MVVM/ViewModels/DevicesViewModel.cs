using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlWPFClient.BusinessLogic.Services;
using RemoteControlWPFClient.MVVM.Command;
using RemoteControlWPFClient.MVVM.Events;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.Models;
using RemoteControlWPFClient.Views.UserControls.Home;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteControlWPFClient.MVVM.ViewModels
{
	public partial class DevicesViewModel : ObservableValidator, ITransient
	{
		private readonly ServerAPIProviderService apiProvider;
		private readonly CurrentUserServices currentUser;
		private readonly EventBus eventBus;

		[ObservableProperty]
		private User user;

		[ObservableProperty]
		private ObservableCollection<Device> connectedDevices;

		public DevicesViewModel(ServerAPIProviderService apiProvider, CurrentUserServices currentUser, EventBus eventBus)
		{
			this.apiProvider = apiProvider;
			this.eventBus = eventBus;
		}


		public ICommand LoadConnectedDevicesCommand => new AwaitableCommand(LoadConnectedDevicesAsync);

		private async Task LoadConnectedDevicesAsync()
		{
			var tokenSource = new CancellationTokenSource(10000);
			ConnectedDevices = new ObservableCollection<Device>(await apiProvider.GetConnectedDeviceAsync(User, tokenSource.Token).ConfigureAwait(false));
			if (ConnectedDevices == null)
			{
				
			}

			//return Task.CompletedTask;
		}
	}
}
