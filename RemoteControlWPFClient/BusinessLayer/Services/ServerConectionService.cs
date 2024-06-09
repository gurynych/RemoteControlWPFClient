using System;
using System.Threading.Tasks;
using NetworkMessage.Communicator;
using RemoteControlWPFClient.WpfLayer.IoC;

namespace RemoteControlWPFClient.BusinessLayer.Services;

public class ServerConectionService : ISingleton
{
    private readonly TcpCryptoClientCommunicator communicator;
    private readonly ServerAPIProvider apiProvider;

    public ServerConectionService(TcpCryptoClientCommunicator communicator)
    {
        this.communicator = communicator ?? throw new ArgumentNullException(nameof(communicator));
    }
    
    public Task ReconnectAsync()
    {
        return communicator.ReconnectWithHandshakeAsync(ServerAPIProvider.ServerAddress, int.Parse(ServerAPIProvider.ServerPort));
    }
}