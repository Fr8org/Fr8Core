namespace Data.Repositories.Encryption.Impl
{
    /// <summary>
    /// Ids of providers to be stored within encrypted data header
    /// Ids are integers to save space in the header
    /// NEVER EVER CHANGE VALUES OF EXISITNG IDs!!! You'll make decryption of exisitng data impossible if you do so.
    /// But you are free to add new Ids for new encryption providers. 
    /// </summary>
    static class EncryptionProviderIds
    {
        public const int Bypass = 1;
        public const int Compressor = 2;
        public const int RinjdaelWithCompression = 3;
        public const int Rinjdael = 4;
    }
}
