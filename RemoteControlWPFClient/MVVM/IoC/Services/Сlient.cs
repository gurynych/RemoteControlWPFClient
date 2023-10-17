using NetworkMessage;
using NetworkMessage.Commands;
using NetworkMessage.CommandsResaults;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlWPFClient.MVVM.IoC;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlServer.BusinessLogic.Services
{
    public class Сlient : TcpClientCryptoCommunicator, ISingleton
    {
        public const string ServerIP = "127.0.0.1";
        public const int ServerPort = 11000;       
        private CancellationTokenSource cancelTokenSrc;

        public Сlient(IAsymmetricCryptographer cryptographer, AsymmetricKeyStoreBase keyStore)
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

        public override void Handshake(CancellationToken token)
        {
            int repeatCount = 0;
            byte[] clientPublicKey = keyStore.GetPublicKey();
            PublicKeyResult publicKeyResult = new PublicKeyResult(clientPublicKey);
            SendPublicKey(publicKeyResult);
            HwidCommand hwidCommand;
            do
            {
                token.ThrowIfCancellationRequested();
                if (repeatCount == 3)
                {
                    throw new SocketException();
                }

                hwidCommand = Receive() as HwidCommand;
                repeatCount++;
            } while (hwidCommand == default);

            INetworkCommandResult hwidResult = hwidCommand.Do().Result;
            Send(hwidResult);
        }
    }
}
