using System;
using NUnit.Framework;
using Utilities;

namespace KwasantTest.Utilities
{
    [TestFixture]
    public class EncryptionTests : BaseTest
    {
        private string _randomString;

        [SetUp]
        public void Setup()
        {
            _randomString = Guid.NewGuid().ToString();
        }

        [Test]
        public void CanEncryptThenDecryptToOriginalValue()
        {
            // SETUP

            // EXECUTE
            var encryptedString = Encryption.Encrypt(_randomString);
            var decryptedString = Encryption.Decrypt(encryptedString);

            // VERIFY
            Assert.AreEqual(_randomString, decryptedString, "Encrypt Utility didn't return the initial string.");
        }
    }
}
