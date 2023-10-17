using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlWPFClient.MVVM.IoC;

namespace RemoteControlWPFClient.BusinessLogic.KeyStore
{
    public class ClientKeyStore : AsymmetricKeyStoreBase
    {
        private readonly byte[] privateKey;

        public ClientKeyStore(IAsymmetricCryptographer cryptographer) : base(cryptographer)
        {
            privateKey = cryptographer.GeneratePrivateKey();
        }

        protected override byte[] SetPrivateKey()
        {
            return privateKey;
        }
    }
}
