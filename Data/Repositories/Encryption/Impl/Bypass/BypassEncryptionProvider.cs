using System.IO;

namespace Data.Repositories.Encryption.Impl
{
    public class BypassEncryptionProvider : IEncryptionProvider
    {
        public int Id { get; } = EncryptionProviderIds.Bypass;
        public int Version { get; } = 1;

        public void EncryptData(Stream encryptedData, Stream sourceData, string peerId)
        {
            sourceData.CopyTo(encryptedData);
        }

        public void DecryptData(Stream encryptedData, Stream decryptedData, string peerId)
        {
            encryptedData.CopyTo(decryptedData);
        }
    }
}
