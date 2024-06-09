using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Helpers;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Events;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Device;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;

namespace RemoteControlWPFClient.WpfLayer.ViewModels;

public partial class DeviceInfoViewModel : ViewModelBase<DeviceInfoUC>, ITransient
{
    private readonly ServerAPIProvider apiProvider;
    private readonly EventBus eventBus;
    private readonly CurrentUserServices currentUser;

    [ObservableProperty] private DeviceDTO device;
    [ObservableProperty] private DeviceStatusesDTO deviceStatuses;

    public DeviceInfoViewModel(ServerAPIProvider apiProvider, EventBus eventBus, 
        CurrentUserServices currentUser, DeviceDTO device, DeviceStatusesDTO deviceStatuses = null)
    {
        this.apiProvider = apiProvider ?? throw new ArgumentNullException(nameof(apiProvider));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        Device = device ?? throw new ArgumentNullException(nameof(device));
        DeviceStatuses = deviceStatuses;
    }


    private ICommand takeScreenshotCommand;
    public ICommand TakeScreenshotCommand => takeScreenshotCommand ??= new AsyncCommand(DownloadScreenshotAsync);

    private ICommand openDeviceFolerCommand;
    public ICommand OpenDeviceFolerCommand => openDeviceFolerCommand ??= new AsyncCommand(OpenDeviceFolderAsync);

    private ICommand openRunningProgramsCommand;
    public ICommand OpenRunningProgramsCommand => openRunningProgramsCommand ??= new AsyncCommand(OpenRunningProgramsAsync);

    private async Task OpenDeviceFolderAsync()
    {
        DeviceFolderUC control = IoCContainer.OpenViewModel<DeviceFolderViewModel, DeviceFolderUC>(Device);
        await eventBus.Publish(new ChangeControlEvent(control, false));
    }
    
    private async Task OpenRunningProgramsAsync()
    {
        RunninProgramsUC control = IoCContainer.OpenViewModel<RunningProgramsViewModel, RunninProgramsUC>(Device);
        await eventBus.Publish(new ChangeControlEvent(control, false));
    }

    private async Task DownloadScreenshotAsync()
    {
        try
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            string downloadDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string screenshotPath = $"Screenshot_{Device?.DeviceName}_{DateTime.Now.ToString("dd_MM_yyyy")}.jpeg";
            screenshotPath = FileSaver.CreateUniqueDownloadPath(screenshotPath, downloadDirPath);

            using (FileStream fs = File.Open(screenshotPath, FileMode.Create))
            {
                await apiProvider.GetScreenshot(currentUser.CurrentUser, Device, fs, tokenSource.Token);
            }

            MessageBoxResult dialogResult = MessageBox.Show("Скриншот сохранен в папку загрузки. Открыть расположение?",
                "Команда выполнена",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialogResult == MessageBoxResult.Yes)
            {
                Process.Start("explorer.exe", downloadDirPath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}