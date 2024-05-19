using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.KeyStore;

namespace RemoteControlWPFClient.BusinessLayer.KeyStore
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
