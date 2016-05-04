namespace Data.Repositories.Encryption
{
    public interface IEncryptionService
    {
        byte[] EncryptData(string peerId, string data);
        byte[] EncryptData(string peerId, byte[] data);
        string DecryptString(string peerId, byte[] encryptedData);
        byte[] DecryptByteArray(string peerId, byte[] encryptedData);
    }
}
