using System;
using System.IO;
using System.Linq;
using Data.Repositories.Encryption;
using Data.Repositories.Encryption.Impl;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;

namespace HubTests.Repositories.Encryption
{
    [TestFixture]
    [Category("EncryptionService")]
    public class EncryptionServiceTests : BaseTest
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        private class CustomEncryptionProvider : IEncryptionProvider
        {
            public int Id { get; } = 10001;
            public int Version { get; } = 1;

            public void EncryptData(Stream encryptedData, Stream sourceData, string peerId)
            {
                encryptedData.WriteByte(128);
                sourceData.CopyTo(encryptedData);
            }

            public void DecryptData(Stream encryptedData, Stream decryptedData, string peerId)
            {
                var flag = encryptedData.ReadByte();

                Assert.AreEqual(128, flag, "Invalid encrypted data flag for CustomEncryptionProvider");

                encryptedData.CopyTo(decryptedData);
            }
        }
        
        private IEncryptionService _encryptionService;
        
        public override void SetUp()
        {
            base.SetUp();
            _encryptionService = ObjectFactory.GetInstance<IEncryptionService>(); 
        }
        
        [Test]
        public void CanEncryptString()
        {
            _encryptionService.EncryptData("peer", "test string");
        }

        [Test]
        public void CanEncryptByteArray()
        {
            _encryptionService.EncryptData("peer", Enumerable.Range(0, 100).Select(x => (byte) x).ToArray());
        }

        [Test]
        public void CanEncryptAndDecryptString()
        {
            var encryptedData = _encryptionService.EncryptData("peer", "test string");
            var decryptedData = _encryptionService.DecryptString("peer", encryptedData);

            Assert.AreEqual("test string", decryptedData, "Decrypted data differs from original");
        }

        [Test]
        public void CanEncryptAndDecryptByteArray()
        {
            var source = Enumerable.Range(0, 100).Select(x => (byte) x).ToArray();
            var encryptedData = _encryptionService.EncryptData("peer", source);
            var decryptedData = _encryptionService.DecryptByteArray("peer", encryptedData);

            Assert.IsTrue(source.SequenceEqual(decryptedData), "Decrypted data differs from original");
        }

        [Test]
        public void CanEncryptWithMultipleEncryptionProvidersDefault()
        {
            var containerA = new Container(x =>
            {
                x.For<IEncryptionService>().Use<EncryptionService>();
                x.For<IEncryptionProvider>().Add<CustomEncryptionProvider>();
                x.For<IEncryptionProvider>().Use<BypassEncryptionProvider>();
            });
            
            var data = containerA.GetInstance<IEncryptionService>().EncryptData("peer", "test string");
            Assert.AreEqual(1, BitConverter.ToInt32(data, 0), "Invalid provier was selected");
        }


        [Test]
        public void CanEncryptWithMultipleEncryptionProvidersCustom()
        {
            var containerA = new Container(x =>
            {
                x.For<IEncryptionService>().Use<EncryptionService>();
                x.For<IEncryptionProvider>().Add<BypassEncryptionProvider>();
                x.For<IEncryptionProvider>().Use<CustomEncryptionProvider>();
            });

            var data = containerA.GetInstance<IEncryptionService>().EncryptData("peer", "test string");
            Assert.AreEqual(10001, BitConverter.ToInt32(data, 0), "Invalid provier was selected");
        }


        [Test]
        public void CanDecryptWithMultipleEncryptionProviders()
        {
            var containerA = new Container(x =>
            {
                x.For<IEncryptionService>().Use<EncryptionService>();
                x.For<IEncryptionProvider>().Add<BypassEncryptionProvider>();
                x.For<IEncryptionProvider>().Use<CustomEncryptionProvider>();
            });

            var dataByCustomProvier = containerA.GetInstance<IEncryptionService>().EncryptData("peer", "test string custom");
            
            var containerB = new Container(x =>
            {
                x.For<IEncryptionService>().Use<EncryptionService>();
                x.For<IEncryptionProvider>().Add<CustomEncryptionProvider>();
                x.For<IEncryptionProvider>().Use<BypassEncryptionProvider>();
            });

            var dataByDefaultProvier = containerB.GetInstance<IEncryptionService>().EncryptData("peer", "test string default");


            Assert.AreEqual("test string custom", containerA.GetInstance<IEncryptionService>().DecryptString("peer", dataByCustomProvier));
            Assert.AreEqual("test string custom", containerB.GetInstance<IEncryptionService>().DecryptString("peer", dataByCustomProvier));

            Assert.AreEqual("test string default", containerA.GetInstance<IEncryptionService>().DecryptString("peer", dataByDefaultProvier));
            Assert.AreEqual("test string default", containerB.GetInstance<IEncryptionService>().DecryptString("peer", dataByDefaultProvier));
        }
    }
}
