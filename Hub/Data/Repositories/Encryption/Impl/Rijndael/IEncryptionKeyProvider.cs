namespace Data.Repositories.Encryption.Impl.Rijndael
{
    public interface IEncryptionKeyProvider
    {
        EncryptionKey GetEncryptionKey(string peerId, int keyLength, int ivLength);
    }
}