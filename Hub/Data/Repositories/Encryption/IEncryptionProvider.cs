using System.IO;

namespace Data.Repositories.Encryption
{
    public interface IEncryptionProvider
    {
        int Id { get; }
        int Version { get; }
        
        void EncryptData(Stream encryptedData, Stream sourceData, string peerId);
        void DecryptData(Stream encryptedData, Stream decryptedData, string peerId);
    }
}
