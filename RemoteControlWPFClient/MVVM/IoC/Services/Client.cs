using NetworkMessage;
using NetworkMessage.Commands;
using NetworkMessage.CommandsResults;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NetworkMessage.Intents;
using NetworkMessage.Windows;
using RemoteControlWPFClient.BusinessLogic.Services;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.MVVM.IoC.Services
{
    public class Client : TcpCommunicator, ISingleton
    {
        //public const string ServerIP = "127.0.0.1";
        public const string ServerIP = ServerAPIProviderService.ServerAddress;
        public const int ServerPort = 11000;
        private CancellationTokenSource cancelTokenSrc;

        public Client(IAsymmetricCryptographer asymmetricCryptographer,
            ISymmetricCryptographer symmetricCryptographer,
            AsymmetricKeyStoreBase keyStore)
            : base(new TcpClient(), asymmetricCryptographer, symmetricCryptographer, keyStore)
        {
            cancelTokenSrc = new CancellationTokenSource();
        }

        public bool Connect()
        {
            try
            {
                client.Connect(ServerIP, ServerPort);
                if (client.Connected)
                {
                    //SendAsync()
                }

                return true;
            }
            catch (Exception)
            {
                //logger.Log();
                return false;
            }
        }

        public async Task<bool> ConnectAsync(CancellationToken token = default)
        {
            try
            {
                if (token != default)
                {
                    await client.ConnectAsync(ServerIP, ServerPort, token);
                    return true;
                }

                return await Task.Run(Connect);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void Stop()
        {
            client.Close();
            cancelTokenSrc.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>        
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override async Task Handshake(CancellationToken token)
        {
            byte[] clientPublicKey = keyStore.GetPublicKey();
            PublicKeyResult publicKeyResult = new PublicKeyResult(clientPublicKey);
            await SendPublicKeyAsync(publicKeyResult, token);
            GuidIntent guidIntent = await ReceiveIntentAsync<GuidIntent>(token);
            if (guidIntent == null) throw new NullReferenceException(nameof(guidIntent));

            WindowsCommandFactory windowsFactory = new WindowsCommandFactory();
            NetworkCommandBase command = guidIntent.CreateCommand(windowsFactory);
            NetworkCommandResultBase guidResult = await command.ExecuteAsync();
            INetworkMessage message = new NetworkMessage.NetworkMessage(guidResult);
            await SendAsync(message, token);
            DownloadFileIntent fileIntent = await ReceiveIntentAsync<DownloadFileIntent>(token);
            command = fileIntent.CreateCommand(windowsFactory);
            NetworkCommandResultBase result = await command.ExecuteAsync();
            message = new NetworkMessage.NetworkMessage(result);
            await SendAsync(message, token);
        }
    }
}
