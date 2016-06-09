using System.IO;
using System.Linq;
using Data.Repositories.Encryption;
using Data.Repositories.Encryption.Impl.Rijndael;
using NUnit.Framework;
using Fr8.Testing.Unit;

namespace HubTests.Repositories.Encryption
{
    [TestFixture]
    [Category("EncryptionService")]
    public class RijndaelEncryptionProviderTests : BaseTest
    {
        private class EcryptionKeyProviderStub : IEncryptionKeyProvider
        {
            public EncryptionKey GetEncryptionKey(string peerId, int keyLength, int ivLength)
            {
                var key = Enumerable.Range(0, keyLength).Select(x => (byte) x).ToArray();
                var iv = Enumerable.Range(0, ivLength).Select(x => (byte) x).ToArray();

                return new EncryptionKey(key, iv);
            }
        }

        private void CanDecryptEncryptedData(IEncryptionProvider provider)
        {
            var sampleData = Enumerable.Range(0, 100).Select(x => (byte)x).ToArray();

            byte[] encryptedData;
            byte[] decryptedData;

            using (var encryptedStream = new MemoryStream())
            using (var sourceStream = new MemoryStream(sampleData))
            {
                provider.EncryptData(encryptedStream, sourceStream, "peer");

                encryptedStream.Flush();
                encryptedData = encryptedStream.ToArray();
            }

            using (var encryptedStream = new MemoryStream(encryptedData))
            using (var decryptedStream = new MemoryStream())
            {
                provider.DecryptData(encryptedStream, decryptedStream, "peer");

                decryptedStream.Flush();
                decryptedData = decryptedStream.ToArray();
            }
            
            Assert.IsTrue(sampleData.SequenceEqual(decryptedData), "Decrypted data is differs from original");
        }

        [Test]
        public void RijndaelCanDecryptEncryptedData()
        {
            var provider = new RijndaelEncryptionProviderV1(new EcryptionKeyProviderStub());
            CanDecryptEncryptedData(provider);

        }

        [Test]
        public void RijndaelWithCompressionCanDecryptEncryptedData()
        {
            var provider = new RijndaelEncryptionProviderWithCompressionV1(new EcryptionKeyProviderStub());
            CanDecryptEncryptedData(provider);

        }

    }
}
