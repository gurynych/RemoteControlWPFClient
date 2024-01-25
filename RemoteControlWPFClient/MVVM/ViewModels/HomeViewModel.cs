using CommunityToolkit.Mvvm.ComponentModel;
using NetworkMessage.CommandFactory;
using NetworkMessage.Communicator;
using RemoteControlWPFClient.BusinessLogic.Services;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public partial class HomeViewModel: ObservableValidator, ITransient
    {
        private readonly ICommandFactory factory;
        private readonly TcpCryptoClientCommunicator communicator;
        private readonly ServerAPIProviderService apiProvider;
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

        public HomeViewModel(TcpCryptoClientCommunicator communicator, ICommandFactory commandFactory, ServerAPIProviderService apiProvider)
        {
            this.communicator = communicator;
            this.factory = commandFactory;
            this.apiProvider = apiProvider;
            IsConneted = communicator.IsConnected;
        }

        
    }
}
