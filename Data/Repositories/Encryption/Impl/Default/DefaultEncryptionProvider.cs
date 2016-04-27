using System.IO;

namespace Data.Repositories.Encryption.Impl
{
    public class DefaultEncryptionProvider : IEncryptionProvider
    {
        public int Id { get; } = 1;
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
