using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Data.Repositories.Encryption.Impl.Rijndael
{
    public class RijndaelEncryptionProviderWithCompressionV1 : RijndaelEncryptionProviderV1
    {
        public override int Id { get; } = EncryptionProviderIds.RinjdaelWithCompression;
        public override int Version { get; } = 1;
        
        public RijndaelEncryptionProviderWithCompressionV1(IEncryptionKeyProvider encryptionKeyProvider)
            :base (encryptionKeyProvider)
        {
        }

        public override void EncryptData(Stream encryptedData, Stream sourceData, string peerId)
        {
            var transform = GetTransform(peerId);

            using (var cryptoStream = new CryptoStream(encryptedData, transform.Encryptor, CryptoStreamMode.Write))
            using (var compressedStream = new GZipStream(cryptoStream, CompressionMode.Compress))
            {
                sourceData.CopyTo(compressedStream);
            }
        }

        public override void DecryptData(Stream encryptedData, Stream decryptedData, string peerId)
        {
            var transform = GetTransform(peerId);

            using (var cryptoStream = new CryptoStream(encryptedData, transform.Decryptor, CryptoStreamMode.Read))
            using (var decompressedStream = new GZipStream(cryptoStream, CompressionMode.Decompress))
            {
                decompressedStream.CopyTo(decryptedData);
            }
        }
    }
}
