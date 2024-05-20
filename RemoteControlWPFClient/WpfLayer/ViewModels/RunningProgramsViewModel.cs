using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DevExpress.Mvvm;
using NetworkMessage.DTO;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.Command;
using RemoteControlWPFClient.WpfLayer.IoC;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;
using RemoteControlWPFClient.WpfLayer.Views.UserControls.Device;

namespace RemoteControlWPFClient.WpfLayer.ViewModels;

public partial class RunningProgramsViewModel : ViewModelBase<RunninProgramsUC>, ITransient
{
    private readonly DeviceDTO device;
    private readonly ServerAPIProvider apiProvider;
    private readonly CurrentUserServices userServices;

    [ObservableProperty] private bool isLoad;
    public ObservableCollection<ProgramInfoDTO> RunningPrograms { get; set; }
    
    public RunningProgramsViewModel(DeviceDTO device, ServerAPIProvider apiProvider, CurrentUserServices userServices)
    {
        this.device = device ?? throw new ArgumentNullException(nameof(device));
        this.apiProvider = apiProvider ?? throw new ArgumentNullException(nameof(apiProvider));
        this.userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        RunningPrograms = new ObservableCollection<ProgramInfoDTO>();
    }

    private ICommand loadRunningProgramsCommand;
    public ICommand LoadRunningProgramsCommand => loadRunningProgramsCommand ??= new AwaitableCommand(LoadRunningProgramsAsync);
    
    private ICommand searchInProgramsCommand;
    public ICommand SearchInProgramsCommand => searchInProgramsCommand ??= new AsyncCommand<string>(SearchInProgramsAsync);

    private async Task LoadRunningProgramsAsync()
    {
        IsLoad = true;
        CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
        try
        { 
            RunningPrograms.Clear();
            List<ProgramInfoDTO> runningPrograms = (await apiProvider.GetRunninProgramsAsync(userServices.CurrentUser, device, tokenSource.Token)).ToList();
            runningPrograms.ForEach(RunningPrograms.Add);
        }
        catch (Exception ex)
        {
            IsLoad = false;
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
        IsLoad = false;
    }
    
    private async Task SearchInProgramsAsync(string searchText)
    {
        IsLoad = true;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            await LoadRunningProgramsAsync();
            IsLoad = false;
            return;
        }
        
        List<ProgramInfoDTO> filtered = RunningPrograms.Where(x =>
            {
                bool nameComparison = x.ProgramName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                bool pathComparison = x.ProgramPath?.Contains(searchText) ?? false;
                bool memoryComparison = false;
                if (long.TryParse(searchText, out long memory))
                {
                    memoryComparison = (long)x.MemoryByte == memory;
                }

                return !(nameComparison || pathComparison || memoryComparison);
            })
            .ToList();

        filtered.ForEach(x => RunningPrograms.Remove(x));
        IsLoad = false;
    }
}