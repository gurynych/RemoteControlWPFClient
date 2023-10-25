using NetworkMessage;
using NetworkMessage.Commands;
using NetworkMessage.CommandsResaults;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.MVVM.IoC.Services
{
    public class Client : TcpClientCryptoCommunicator, ISingleton
    {
        public const string ServerIP = "127.0.0.1";
        public const int ServerPort = 11000;       
        private CancellationTokenSource cancelTokenSrc;

        public Client(IAsymmetricCryptographer cryptographer, AsymmetricKeyStoreBase keyStore)
            : base(new TcpClient(), cryptographer, keyStore)
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
            //s->r->s            
            byte[] clientPublicKey = keyStore.GetPublicKey();
            PublicKeyResult publicKeyResult = new PublicKeyResult(clientPublicKey);
            await SendPublicKeyAsync(publicKeyResult, token);

            INetworkObject networkObject = await ReceiveAsync(token);
            if (networkObject is INetworkCommand command)
            {
                INetworkCommandResult hwidResult = await command.Do();
                await SendAsync(hwidResult, token);
            }
            else throw new NullReferenceException(nameof(networkObject));
        }
    }
}
