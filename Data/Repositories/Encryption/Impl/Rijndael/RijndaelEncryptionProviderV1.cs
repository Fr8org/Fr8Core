using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Data.Repositories.Encryption.Impl.Rijndael
{
    public class RijndaelEncryptionProviderV1 : IEncryptionProvider
    {
        protected class CryptoTransform
        {
            public readonly ICryptoTransform Encryptor;
            public readonly ICryptoTransform Decryptor;

            public CryptoTransform(ICryptoTransform encryptor, ICryptoTransform decryptor)
            {
                Encryptor = encryptor;
                Decryptor = decryptor;
            }
        }

        private readonly Dictionary<string, CryptoTransform> _transforms = new Dictionary<string, CryptoTransform>();
        private readonly IEncryptionKeyProvider _encryptionEncryptionKeyProvider;

        protected RijndaelManaged CryptoProvider { get; }

        public virtual int Id { get; } = EncryptionProvidersIds.Rinjdael;
        public virtual int Version { get; } = 1;
        
        public RijndaelEncryptionProviderV1(IEncryptionKeyProvider encryptionKeyProvider)
        {
            CryptoProvider = new RijndaelManaged();
            _encryptionEncryptionKeyProvider = encryptionKeyProvider;
        }

        public virtual void EncryptData(Stream encryptedData, Stream sourceData, string peerId)
        {
            var transform = GetTransform(peerId);

            using (var cryptoStream = new CryptoStream(encryptedData, transform.Encryptor, CryptoStreamMode.Write))
            {
                sourceData.CopyTo(cryptoStream);
            }
        }

        public virtual void DecryptData(Stream encryptedData, Stream decryptedData, string peerId)
        {
            var transform = GetTransform(peerId);

            using (var cryptoStream = new CryptoStream(encryptedData, transform.Decryptor, CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(decryptedData);
            }
        }

        protected CryptoTransform GetTransform(string peerId)
        {
            CryptoTransform transform;

            lock (_transforms)
            {
                if (!_transforms.TryGetValue(peerId, out transform))
                {
                    var key =_encryptionEncryptionKeyProvider.GetEncryptionKey(peerId, CryptoProvider.KeySize / 8, CryptoProvider.BlockSize / 8);

                    transform = new CryptoTransform(CryptoProvider.CreateEncryptor(key.Key, key.IV), 
                                                    CryptoProvider.CreateDecryptor(key.Key, key.IV));
                    _transforms.Add(peerId, transform);
                }
            }

            return transform;
        }
    }
}
