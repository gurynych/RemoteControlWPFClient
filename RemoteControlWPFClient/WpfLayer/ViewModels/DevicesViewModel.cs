using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Device;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Home;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.Events;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;
using MessageBox = System.Windows.MessageBox;

namespace RemoteControlWPFClient.WpfLayer.ViewModels
{
    public partial class DevicesViewModel : ValidatableViewModelBase<DevicesUC>, ITransient
    {
        private readonly ServerAPIProvider apiProvider;
        private readonly CurrentUserServices currentUser;
        private readonly EventBus eventBus;

        [ObservableProperty] private UserDTO userDto;
        [ObservableProperty] private ObservableCollection<DeviceDTO> connectedDevices;

        public DevicesViewModel(ServerAPIProvider apiProvider, CurrentUserServices currentUser, EventBus eventBus)
        {
            this.apiProvider = apiProvider ?? throw new ArgumentNullException(nameof(apiProvider));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            UserDto = currentUser.CurrentUser;
            ConnectedDevices = new ObservableCollection<DeviceDTO>();
        }

        private ICommand loadConnectedDevicesCommand;
        public ICommand LoadConnectedDevicesCommand =>
            loadConnectedDevicesCommand ??= new AwaitableCommand(LoadConnectedDevicesAsync);

        private async Task LoadConnectedDevicesAsync()
        {
            ConnectedDevices.Clear();
            var tokenSource = new CancellationTokenSource(10000);
            List<DeviceDTO> devices = await apiProvider.GetConnectedDeviceAsync(UserDto, tokenSource.Token);
            devices?.ForEach(ConnectedDevices.Add);
        }

        private ICommand openDeviceCommand;
        public ICommand OpenDeviceCommand => openDeviceCommand ??= new AwaitableCommand<DeviceDTO>(LoadDeviceAsync);

        private async Task LoadDeviceAsync(DeviceDTO device)
        {
            CancellationTokenSource tokenSource = null;
            try
            {
                DeviceStatusesDTO deviceStatuses = null;
                if (device.IsConnected)
                {
                    tokenSource = new CancellationTokenSource(50000);
                    deviceStatuses =
                        await apiProvider.GetDeviceStatuses(currentUser.CurrentUser, device, tokenSource.Token);
                }

                DeviceInfoUC control =
                    IoCContainer.OpenViewModel<DeviceInfoViewModel, DeviceInfoUC>(device, deviceStatuses);
                await eventBus.Publish(new ChangeControlEvent(control, false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                tokenSource?.Cancel();
                tokenSource?.Dispose(); 
            }
        }
    }
}