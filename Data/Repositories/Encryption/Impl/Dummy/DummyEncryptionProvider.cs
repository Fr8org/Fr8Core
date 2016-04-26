using System;
using System.Text;

namespace Data.Repositories.Encryption.Impl
{
    public class DummyEncryptionProvider : IEncryptionProvider
    {
        public byte[] EncryptData(string peerId, string data)
        {
            if (data == null)
            {
                return null;
            }

            return Encoding.Default.GetBytes(peerId + data);
        }

        public byte[] EncryptData(string peerId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public string DecryptString(string peerId, byte[] encryptedData)
        {
            if (encryptedData == null)
            {
                return null;
            }

            var decryptedString = Encoding.Default.GetString(encryptedData);

            if (string.IsNullOrEmpty(peerId))
            {
                return decryptedString;
            }

            if (!decryptedString.StartsWith(peerId))
            {
                throw new InvalidOperationException("Can't decrypt data because it is belong to another user");
            }
            
            return decryptedString.Substring(peerId.Length);
        }

        public byte[] DecryptByteArray(string peerId, byte[] encryptedData)
        {
            throw new NotImplementedException();
        }
    }
}
