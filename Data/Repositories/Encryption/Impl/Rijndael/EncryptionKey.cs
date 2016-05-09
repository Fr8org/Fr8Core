using System;

namespace Data.Repositories.Encryption.Impl.Rijndael
{
    public class EncryptionKey
    {
        public readonly byte[] Key;
        public readonly byte[] IV;

        public EncryptionKey(byte[] key, byte[] iv)
        {
            Key = key;
            IV = iv;
        }

        public static EncryptionKey FromByteArray(byte[] bytes)
        {
            var keySize = BitConverter.ToInt32(bytes, 0);
            var ivSize = BitConverter.ToInt32(bytes, 4);
            var key = new byte[keySize];
            var iv = new byte[ivSize];

            Buffer.BlockCopy(bytes, 8, key, 0, key.Length);
            Buffer.BlockCopy(bytes, 8 + key.Length, iv, 0, iv.Length);

            return new EncryptionKey(key, iv);
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[Key.Length + IV.Length + 8];
            
            Buffer.BlockCopy(BitConverter.GetBytes(Key.Length), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(IV.Length), 0, bytes, 4, 4);

            Buffer.BlockCopy(Key, 0, bytes, 8, Key.Length);
            Buffer.BlockCopy(IV, 0, bytes, 8 + Key.Length, IV.Length);

            return bytes;
        }
    }
}