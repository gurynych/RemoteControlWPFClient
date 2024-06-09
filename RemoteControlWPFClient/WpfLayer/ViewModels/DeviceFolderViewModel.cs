using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm;
using NetworkMessage.DTO;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Helpers;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Device;
using FileInfoDTO = RemoteControlWPFClient.BusinessLayer.DTO.FileInfoDTO;

namespace RemoteControlWPFClient.WpfLayer.ViewModels;

public partial class DeviceFolderViewModel : ViewModelBase<DeviceFolderUC>, ITransient
{
    private string path;
    private IProgress<long> progress;
    private readonly CurrentUserServices userServices;
    private readonly ServerAPIProvider apiProvider;
    private readonly DeviceDTO device;

    [ObservableProperty] private string fullPath;
    [ObservableProperty] private string searchText;
    [ObservableProperty] private bool isLoad;
    [ObservableProperty] private int totalFilesAmount;
    
    [ObservableProperty] private bool isDownload;
    [ObservableProperty] private string downloadableFileName;
    [ObservableProperty] private long downloadableBytesAmount;
    [ObservableProperty] private long totalBytesAmount;
    [ObservableProperty] private int percentDownloaded;
    
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
            TotalFilesAmount = FileInfoList.Count; 
            FullPath = string.Join('\\', OpenedDirectoryHistory);
            OnPropertyChanged(nameof(OpenedDirectoryHistory));
        };
    }

    private ICommand loadRootDirectoryCommand;
    public ICommand LoadRootDirectoryCommand => loadRootDirectoryCommand ??= new AsyncCommand(LoadFilesInRootAsync);

    private ICommand openDirectoryCommand;
    public ICommand OpenDriveOrDirectoryCommand => openDirectoryCommand ??= new AsyncCommand<FileInfoDTO>(OpenDriveOrDirectoryAsync);

    private ICommand openPreviousDirectoryCommand;

    public ICommand OpenPreviousDirectoryCommand =>
        openPreviousDirectoryCommand ??= new AsyncCommand(BackInFolderAsync);

    private ICommand refreshDirectoryCommand;
    public ICommand RefreshDirectoryCommand => refreshDirectoryCommand ??= new AwaitableCommand(RefreshDirectoryAsync);

    private ICommand searchInFilesCommand;
    public ICommand SearchInFilesCommand => searchInFilesCommand ??= new AsyncCommand(SearchInFilesAsync);
    
    private ICommand openByUserPathDirectoryCommand;
    public ICommand OpenByUserPathDirectoryCommand => openByUserPathDirectoryCommand ??= new AsyncCommand(OpenByUserPathDirectoryAsync);

    private ICommand downloadItemCommand;
    public ICommand DownloadItemComamnd => downloadItemCommand ??= new AsyncCommand<FileInfoDTO>(DownloadItemAsync);

    private Task DownloadItemAsync(FileInfoDTO fileInfo)
    {
        return fileInfo.FileType switch
        {
            FileType.File => DownloadFileAsync(fileInfo),
            FileType.Directory => DownloadDirectoryAsync(fileInfo),
            _ => Task.CompletedTask
        };
    }
    
    private async Task LoadFilesInRootAsync()
    {
        if (IsLoad) return;
        
        IsLoad = true;

        path = "root";
        OpenedDirectoryHistory.Clear();
        OpenedDirectoryHistory.Add(device.DeviceName);
        await TryLoadNestedFilesInDirectory(path);

        IsLoad = false;
    }

    private async Task OpenDriveOrDirectoryAsync(FileInfoDTO fileInfo)
    {
        if (fileInfo == null || fileInfo.FileType == FileType.File  || IsLoad)
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
        if (IsLoad) return;
        
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
        if (IsLoad) return;
        IsLoad = true;

        await TryLoadNestedFilesInDirectory(path);

        IsLoad = false;
    }

    private async Task SearchInFilesAsync()
    {
        if (IsLoad) return;
        
        IsLoad = true;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await TryLoadNestedFilesInDirectory(path);
            IsLoad = false;
            return;
        }

        List<FileInfoDTO> filtered = FileInfoList.Where(x =>
            {
                bool nameComparison = x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                bool dateComparison = false;
                if (x.ChangingDate.HasValue)
                {
                    dateComparison = x.ChangingDate.Value.ToString("dd.MM.yyyy HH:mm").Contains(SearchText);
                }

                bool fileLengthComparison = false;
                if (int.TryParse(SearchText, out int fileLength))
                {
                    fileLengthComparison = x.FileLength == fileLength;
                }

                return !(nameComparison || dateComparison || fileLengthComparison);
            })
            .ToList();

        filtered.ForEach(x => FileInfoList.Remove(x));

        IsLoad = false;
    }

    private async Task OpenByUserPathDirectoryAsync()
    {
        if (string.IsNullOrWhiteSpace(FullPath) || FullPath.Length < device.DeviceName.Length  || IsLoad || IsDownload) return;
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

    private async Task DownloadFileAsync(FileInfoDTO fileInfo)
    {
        if (fileInfo == null || fileInfo.FileType != FileType.File || IsLoad || IsDownload) return;
        IsDownload = true;

        string downloadDirPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string downloadPath = FileSaver.CreateUniqueDownloadPath(fileInfo.Name, downloadDirPath);
        CancellationTokenSource tokenSource = new CancellationTokenSource(1000000);
        try
        {
            DownloadableFileName = fileInfo.Name;
            TotalBytesAmount = fileInfo.FileLength!.Value;
            progress = new Progress<long>(value =>
            {
                DownloadableBytesAmount = value;
                PercentDownloaded = (int)(value * 1.0 / fileInfo.FileLength * 100);
            });
            
            using (FileStream fs = File.OpenWrite(downloadPath))
            {
                await apiProvider.DownloadFileAsync(userServices.CurrentUser, device, fileInfo.FullName, fs, progress, tokenSource.Token);
            }
            
            IsDownload = false;
            
            MessageBoxResult dialogResult = MessageBox.Show("Файл сохранен в папку загрузки. Открыть расположение?",
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
            IsDownload = false;
        }
        finally
        {
            await tokenSource.CancelAsync();
            tokenSource.Dispose();
        }
    }

    private async Task DownloadDirectoryAsync(FileInfoDTO fileInfo)
    {
        if (fileInfo == null || fileInfo.FileType != FileType.Directory || IsLoad || IsDownload) return;
        IsLoad = true;

        string downloadDirPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string downloadPath = FileSaver.CreateUniqueDownloadPath($"{fileInfo.Name}.rar", downloadDirPath);
        CancellationTokenSource tokenSource = new CancellationTokenSource(1000000);
        try
        {
            using (FileStream fs = File.OpenWrite(downloadPath))
            {
                await apiProvider.DownloadDirectoryAsync(userServices.CurrentUser, device, fileInfo.FullName, fs, progress, tokenSource.Token);
            }
            
            IsLoad = false;
            
            MessageBoxResult dialogResult = MessageBox.Show("Архив сохранен в папку загрузки. Открыть расположение?",
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
            IsLoad = false;
        }
        finally
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
    }
}