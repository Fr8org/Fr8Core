using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using StructureMap;

namespace Data.Repositories.Encryption.Impl
{
    public class EncryptionService : IEncryptionService
    {
        // Id+Version pair represented as the struct with equality members to use as dictionary's key
        private struct EncryptionProviderKey
        {
            public readonly int Id;
            public readonly int Version;

            public EncryptionProviderKey(int id, int version)
            {
                Id = id;
                Version = version;
            }

            private bool Equals(EncryptionProviderKey other)
            {
                return Id == other.Id && Version == other.Version;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is EncryptionProviderKey && Equals((EncryptionProviderKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id*397) ^ Version;
                }
            }
        }

        // type codes for encrypted data
        private static readonly Dictionary<Type, int> SupportedTypeCodes = new Dictionary<Type, int>
        {
            {typeof(string), 1},
            {typeof(byte[]), 2}
        };

        private readonly Dictionary<EncryptionProviderKey, IEncryptionProvider> _encryptionProviders = new Dictionary<EncryptionProviderKey, IEncryptionProvider>();
        private readonly IEncryptionProvider _defaultEncryptionProvider;
        private readonly IContainer _container;

        public EncryptionService(IEncryptionProvider defaultEncryptionProvider, IContainer container)
        {
            _defaultEncryptionProvider = defaultEncryptionProvider;
            _container = container;
            RegisterEncryptionProvider(_defaultEncryptionProvider);
        }

        private void RegisterEncryptionProvider(IEncryptionProvider encryptionProvider)
        {
            _encryptionProviders[new EncryptionProviderKey(encryptionProvider.Id, encryptionProvider.Version)] =  encryptionProvider;
        }

        // here we are trying to find encryption provider with given Id and Version (they are stored within EncryptionProviderKey)
        private IEncryptionProvider ResolveDecryptionProvider(EncryptionProviderKey key)
        {
            IEncryptionProvider provider;

            lock (_encryptionProviders)
            {
                // If there is no provider in cache
                if (!_encryptionProviders.TryGetValue(key, out provider))
                {
                    // list all implementations of IEncryptionProvider within container.
                    var availableProviders = _container.GetAllInstances<IEncryptionProvider>();

                    foreach (var encryptionProvider in availableProviders)
                    {
                        // add them into cache
                        RegisterEncryptionProvider(encryptionProvider);

                        // and check if this provider is what we are looking for
                        if (encryptionProvider.Id == key.Id && encryptionProvider.Version == key.Version)
                        {
                            provider = encryptionProvider;
                        }
                    }
                }
            }

            if (provider == null)
            {
                throw new Exception($"Unable to resolve EncryptionProvider with Id = {key.Id} and Version = {key.Version}");
            }

            return provider;
        }
        
        private Stream DecryptData(string peerId, byte[] encryptedData, out int typeCode)
        {
            if (encryptedData == null)
            {
                typeCode = -1;
                return null;
            }

            if (encryptedData.Length < 12)
            {
                throw new ArgumentException($"Encrypted data has invalid size {encryptedData.Length}. Can't be less than 12 bytes.");
            }

            // read encrypted data header (12 bytes)
            // Using marshaling instead of BitConverter will result into more performant (and somewhat more clear) code, but this requieres Full Trust for execution. 
            // Lets use BitConverter for now
            var providerKey = new EncryptionProviderKey(
                // First 4 bytes are 32-bit integer id of the encryption provider used to encrypt data
                BitConverter.ToInt32(encryptedData, 0),
                // Second 4 bytes are 32-bit integer version of  the encryption provider used to encrypt data
                BitConverter.ToInt32(encryptedData, 4));

            // Third 4 bytes are 32-integer represending type code of the data being encrypted
            typeCode = BitConverter.ToInt32(encryptedData, 8);
            var decryptedDataStream = new MemoryStream();

            // there is no encrypted data. 
            if (encryptedData.Length == 12)
            {
                return decryptedDataStream;
            }

            var provider = ResolveDecryptionProvider(providerKey);
            
            using (var encryptedDataStream = new MemoryStream(encryptedData, 12, encryptedData.Length - 12, false))
            { 
                provider.DecryptData(encryptedDataStream, decryptedDataStream, peerId);
                decryptedDataStream.Seek(0, SeekOrigin.Begin);

                return decryptedDataStream;
            }
        }
        
        private byte[] EncryptData(string peerId, byte[] data, Type type)
        {
            int typeCode;

            if (!SupportedTypeCodes.TryGetValue(type, out typeCode))
            {
                throw new ArgumentException($"Can't encrypt data of unsupported type {type}.");
            }

            using (var encryptedDataStream = new MemoryStream())
            {
                //write encrypted data header (12 bytes)
                // Using marshaling instead of BinaryWriter will result into more performant (and somewhat more clear) code, but this requieres Full Trust for execution. 
                // Lets use BinaryWriter for now
                using (var writer = new BinaryWriter(encryptedDataStream, Encoding.UTF8, true))
                {
                    // First 4 bytes are 32-bit integer id of the encryption provider used to encrypt data
                    writer.Write(_defaultEncryptionProvider.Id);
                    // Second 4 bytes are 32-bit integer version of  the encryption provider used to encrypt data
                    writer.Write(_defaultEncryptionProvider.Version);
                    // Third 4 bytes are 32-integer represending type code of the data being encrypted
                    writer.Write(typeCode);
                }

                if (data != null)
                {
                    using (var sourceDataStream = new MemoryStream(data))
                    {
                        //we always use defaul provider to encrypt the data for now
                        _defaultEncryptionProvider.EncryptData(encryptedDataStream, sourceDataStream, peerId);
                    }
                }

                return encryptedDataStream.ToArray();
            }
        }

        // check if typeCodeToValidate represents type T 
        private void ValidateTypeCode<T>(int typeCodeToValidate)
        {
            int typeCode;

            if (!SupportedTypeCodes.TryGetValue(typeof(T), out typeCode))
            {
                throw new ArgumentException($"Can't decrypt data of unsupported type {typeof(T)}.");
            }

            if (typeCode != typeCodeToValidate)
            {
                throw new ArgumentException($"Can't decrypt data with typeCode {typeCodeToValidate} as type {typeof(T)}.");
            }
        }

        public string DecryptString(string peerId, byte[] encryptedData)
        {
            int typeCode;

            using (var data = DecryptData(peerId, encryptedData, out typeCode))
            {
                ValidateTypeCode<string>(typeCode);

                using (var reader = new StreamReader(data, Encoding.UTF8, true))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public byte[] DecryptByteArray(string peerId, byte[] encryptedData)
        {
            int typeCode;

            using (var data = DecryptData(peerId, encryptedData, out typeCode))
            {
                ValidateTypeCode<byte[]>(typeCode);

                using (var reader = new BinaryReader(data, Encoding.UTF8, true))
                {
                    return reader.ReadBytes((int)data.Length);
                }
            }
        }

        public byte[] EncryptData(string peerId, string data)
        {
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream, Encoding.UTF8))
            {
                writer.Write(data);
                writer.Flush();

                return EncryptData(peerId, memStream.ToArray(), typeof (string));
            }
        }

        public byte[] EncryptData(string peerId, byte[] data)
        {
            return EncryptData(peerId, data, typeof (byte[]));
        }
    }
}
