using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Device;

namespace RemoteControlWPFClient.WpfLayer.ViewModels;

public partial class DeviceFolderViewModel : ViewModelBase<DeviceFolderUC>, ITransient
{
    private string path;
    private readonly CurrentUserServices userServices;
    private readonly ServerAPIProvider apiProvider;
    private readonly DeviceDTO device;

    [ObservableProperty] private string fullPath;
    [ObservableProperty] private string searchText;
    [ObservableProperty] private bool isLoad;
    public ObservableCollection<string> OpenedDirectoryHistory { get; }
    public ObservableCollection<FileInfoDTO> FileInfoList { get; }

    public DeviceFolderViewModel(CurrentUserServices userServices, ServerAPIProvider apiProvider, DeviceDTO device)
    {
        this.userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        this.apiProvider = apiProvider ?? throw new ArgumentNullException(nameof(apiProvider));
        this.device = device ?? throw new ArgumentNullException(nameof(device));
        OpenedDirectoryHistory = new ObservableCollection<string>();
        FileInfoList = new ObservableCollection<FileInfoDTO>();
        OpenedDirectoryHistory.CollectionChanged += (_, _) =>
        {
            FullPath = string.Join('\\', OpenedDirectoryHistory);
            OnPropertyChanged(nameof(OpenedDirectoryHistory));
        };
    }

    private ICommand loadRootDirectoryCommand;
    public ICommand LoadRootDirectoryCommand => loadRootDirectoryCommand ??= new AsyncCommand(LoadFilesInRootAsync);

    private ICommand openDirectoryCommand;
    public ICommand OpenDirectoryCommand => openDirectoryCommand ??= new AsyncCommand<FileInfoDTO>(OpenDirectoryAsync);

    private ICommand openPreviousDirectoryCommand;

    public ICommand OpenPreviousDirectoryCommand =>
        openPreviousDirectoryCommand ??= new AsyncCommand(BackInFolderAsync);

    private ICommand refreshDirectoryCommand;
    public ICommand RefreshDirectoryCommand => refreshDirectoryCommand ??= new AwaitableCommand(RefreshDirectoryAsync);

    private ICommand searchInFilesCommand;
    public ICommand SearchInFilesCommand => searchInFilesCommand ??= new AsyncCommand(SearchInFilesAsync);
    
    private ICommand openByUserPathDirectoryCommand;
    public ICommand OpenByUserPathDirectoryCommand => openByUserPathDirectoryCommand ??= new AsyncCommand(OpenByUserPathDirectoryAsync);

    private async Task LoadFilesInRootAsync()
    {
        IsLoad = true;

        path = "root";
        OpenedDirectoryHistory.Clear();
        OpenedDirectoryHistory.Add(device.DeviceName);
        await TryLoadNestedFilesInDirectory(path);

        IsLoad = false;
    }

    private async Task OpenDirectoryAsync(FileInfoDTO fileInfo)
    {
        if (fileInfo.FileType != FileType.Directory)
        {
            return;
        }

        IsLoad = true;

        this.path += "/" + fileInfo.Name;
        OpenedDirectoryHistory.Add(fileInfo.Name);
        await TryLoadNestedFilesInDirectory(path);

        IsLoad = false;
    }

    private async Task BackInFolderAsync()
    {
        string previousDirectoryName = OpenedDirectoryHistory.LastOrDefault();
        if (previousDirectoryName == null) return;

        IsLoad = true;

        OpenedDirectoryHistory.Remove(previousDirectoryName);
        int prevDirIndex = path.IndexOf(previousDirectoryName);
        path = path[0..(prevDirIndex - 1)];
        if (!OpenedDirectoryHistory.Any()) OpenedDirectoryHistory.Add(device.DeviceName);
        await TryLoadNestedFilesInDirectory(path);

        IsLoad = false;
    }

    private async Task RefreshDirectoryAsync()
    {
        IsLoad = true;

        await TryLoadNestedFilesInDirectory(path);

        IsLoad = false;
    }

    private async Task SearchInFilesAsync()
    {
        IsLoad = true;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await TryLoadNestedFilesInDirectory(path);
            IsLoad = false;
            return;
        }

        List<FileInfoDTO> filtered = FileInfoList.Where(x =>
            {
                bool nameComparison = x.Name.Contains(SearchText);
                bool dateComparison = false;
                if (x.ChangingDate.HasValue)
                {
                    dateComparison = x.ChangingDate.Value.ToString("dd.MM.yyyy HH:mm").Contains(SearchText);
                }

                bool fileLengthComparison = false;
                if (int.TryParse(SearchText, out int fileLength))
                {
                    fileLengthComparison = x.FileLengthMb == fileLength;
                }

                return !(nameComparison || dateComparison || fileLengthComparison);
            })
            .ToList();

        filtered.ForEach(x => FileInfoList.Remove(x));

        IsLoad = false;
    }

    private async Task OpenByUserPathDirectoryAsync()
    {
        if (string.IsNullOrWhiteSpace(FullPath) || FullPath.Length < device.DeviceName.Length) return;
        IsLoad = true;
        
        path = "root" + FullPath[device.DeviceName.Length..];
        await TryLoadNestedFilesInDirectory(path);
        List<string> splited = FullPath.Split('\\', StringSplitOptions.RemoveEmptyEntries).ToList();
        OpenedDirectoryHistory.Clear();
        splited.ForEach(OpenedDirectoryHistory.Add);
        
        IsLoad = false;
    }

    private async Task TryLoadNestedFilesInDirectory(string dirPath)
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
        FileInfoList.Clear();
        try
        {
            await foreach (FileInfoDTO file in apiProvider.GetNestedFilesInfoInDirectoryAsync(userServices.CurrentUser,
                               device, dirPath, tokenSource.Token))
            {
                FileInfoList.Add(file);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
    }

    /*private async Task DownloadFileAsync(FileInfoDTO fileInfo)
    {
        IsLoad = true;
        string pathForDowload = this.path + "/" + fileInfo.Name;
        FileStream fs = File.Create(Path.GetTempPath(), int.MaxValue, FileOptions.Asynchronous);
        CancellationTokenSource tokenSource = new CancellationTokenSource(1000000);
        byte[] fileBytes = await apiProvider.DownloadFileAsync(User, Device.Id, pathForDowload, tokenSource.Token);
        if (fileBytes != null)
        {
            FilesService fileService = new FilesService();
            bool isSuccess = await fileService.SaveFileAsync(fileBytes, name);
            if (isSuccess)
            {
                await Toast.Make("Файл сохранен в папке загрузки", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            }
            else
            {
                await Toast.Make("Ошибка сохранения файла", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            }
        }

        IsLoad = false;
    }*/
}