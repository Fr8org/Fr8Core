using System.Text;

namespace Data.Repositories.Encryption.Impl
{
    public class DefaultEncryptionProvider : IEncryptionProvider
    {
        public byte[] EncryptData(string peerId, string data)
        {
            if (data == null)
            {
                return null;
            }

            return Encoding.Default.GetBytes(data);
        }
        
        public string DecryptString(string peerId, byte[] encryptedData)
        {
            if (encryptedData == null)
            {
                return null;
            }

            var decryptedString = Encoding.Default.GetString(encryptedData);
           
            return decryptedString;
        }

        public byte[] EncryptData(string peerId, byte[] data)
        {
            return data;
        }

        public byte[] DecryptByteArray(string peerId, byte[] encryptedData)
        {
            return encryptedData;
        }
    }
}
