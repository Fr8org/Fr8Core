using System.IO;
using System.IO.Compression;

namespace Data.Repositories.Encryption.Impl
{
    public class CompressingEncryptionProvider : IEncryptionProvider
    {
        public int Id { get; } = EncryptionProviderIds.Compressor;
        public int Version { get; } = 1;

        public void EncryptData(Stream encryptedData, Stream sourceData, string peerId)
        {
            using (var compressedStream = new GZipStream(encryptedData, CompressionMode.Compress))
            {
                sourceData.CopyTo(compressedStream);
            }
        }

        public void DecryptData(Stream encryptedData, Stream decryptedData, string peerId)
        {
            using (var compressedStream = new GZipStream(decryptedData, CompressionMode.Decompress))
            {
                encryptedData.CopyTo(compressedStream);
            }
        }
    }
}
