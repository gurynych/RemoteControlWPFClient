
using NetworkMessage.CommandFactory;
using NetworkMessage.Commands;
using NetworkMessage.CommandsResults;
using NetworkMessage.CommandsResults.ConcreteCommandResults;
using NetworkMessage.Communicator;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NetworkMessage.Intents.ConcreteIntents;
using RemoteControlWPFClient.MVVM.IoC;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.BusinessLogic.Services
{
    public class Client : TcpCryptoClientCommunicator, ISingleton
    {
        public const string ServerIP = ServerAPIProviderService.ServerAddress;
        public const int ServerPort = 11000;
        private ICommandFactory factory;

        public Client(IAsymmetricCryptographer asymmetricCryptographer,
            ISymmetricCryptographer symmetricCryptographer,
            AsymmetricKeyStoreBase keyStore, ICommandFactory commandFactory)
            : base(new TcpClient(), asymmetricCryptographer, symmetricCryptographer, keyStore)
        {
            factory = commandFactory;
        }

        public override async Task<bool> HandshakeAsync(IProgress<long> progress = null, CancellationToken token = default)
        {
            try
            {
                BaseNetworkCommandResult result;
                byte[] publicKey = keyStore.GetPublicKey();
                result = new PublicKeyResult(publicKey);
                await SendObjectAsync(result, progress, token).ConfigureAwait(false);

                GuidIntent guidIntent = await ReceiveAsync<GuidIntent>(progress, token);
                if (guidIntent == null) throw new NullReferenceException(nameof(guidIntent));

                INetworkCommand command = guidIntent.CreateCommand(factory);
                result = await command.ExecuteAsync();
                await SendObjectAsync(result, progress, token);

                SuccessfulTransferResult transferResult =
                    await ReceiveAsync<SuccessfulTransferResult>(token: token);
                return IsConnected = transferResult.IsSuccessful;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                return IsConnected = false;
            }


        }
    }

}
