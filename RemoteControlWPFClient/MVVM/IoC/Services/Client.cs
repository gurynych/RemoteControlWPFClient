﻿using NetworkMessage;
using NetworkMessage.CommandFactory;
using NetworkMessage.Commands;
using NetworkMessage.CommandsResults;
using NetworkMessage.Communicator;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NetworkMessage.Intents;
using NetworkMessage.Windows;
using RemoteControlWPFClient.BusinessLogic.Services;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.MVVM.IoC.Services
{
    public class Client : TcpCryptoClientCommunicator, ISingleton
    {
        public const string ServerIP = ServerAPIProviderService.ServerAddress;
        public const int ServerPort = 11000;
        private ICommandFactory factory;

        public Client(IAsymmetricCryptographer asymmetricCryptographer,
            ISymmetricCryptographer symmetricCryptographer,
            AsymmetricKeyStoreBase keyStore,ICommandFactory commandFactory)
            : base(new TcpClient(), asymmetricCryptographer, symmetricCryptographer, keyStore)
        {
            this.factory = commandFactory;
        }

        public override async Task<bool> HandshakeAsync(IProgress<long> progress = null, CancellationToken token = default)
        {
            try
            {
                BaseNetworkCommandResult result;
                byte[] publicKey = keyStore.GetPublicKey();
                result = new PublicKeyResult(publicKey);
                await SendMessageAsync(result, progress, token).ConfigureAwait(false);

                GuidIntent guidIntent = await ReceiveNetworkObjectAsync<GuidIntent>(progress, token);
                if (guidIntent == null) throw new NullReferenceException(nameof(guidIntent));

                BaseNetworkCommand command = guidIntent.CreateCommand(factory);
                result = await command.ExecuteAsync();
                await SendMessageAsync(result, progress, token);

                SuccessfulTransferResult transferResult =
                    await ReceiveNetworkObjectAsync<SuccessfulTransferResult>(token: token);
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
