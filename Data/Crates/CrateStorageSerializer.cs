using System;
using System.Collections.Generic;
using System.Reflection;
using Data.Interfaces.DataTransferObjects;

namespace Data.Crates
{
    public partial class CrateStorageSerializer : ICrateStorageSerializer
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        public static readonly CrateStorageSerializer Default = new CrateStorageSerializer();

        /**********************************************************************************/

        private readonly Dictionary<CrateManifestType, IManifestSerializer> _serializers = new Dictionary<CrateManifestType, IManifestSerializer>();

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        private CrateStorageSerializer()
        {
        }

        /**********************************************************************************/

        public void RegisterSerializer(CrateManifestType manifestType, IManifestSerializer serializer)
        {
            lock (_serializers)
            {
                _serializers.Add(manifestType, serializer);
            }
        }

        /**********************************************************************************/

        private IManifestSerializer GetSerializer(CrateManifestType type)
        {
            lock (_serializers)
            {
                IManifestSerializer serializer;

                if (_serializers.TryGetValue(type, out serializer))
                {
                    return serializer;
                }

                Type clrType;

                if (ManifestDiscovery.Default.TryResolveType(type, out clrType))
                {
                    var manifestAttr = clrType.GetCustomAttribute<CrateManifestAttribute>();

                    if (manifestAttr == null || manifestAttr.Serializer == null)
                    {
                        serializer = new DefaultSerializer(clrType);
                    }
                    else
                    {
                        if (typeof (IManifestSerializer).IsAssignableFrom(manifestAttr.Serializer) &&
                            manifestAttr.Serializer.GetConstructor(Type.EmptyTypes) != null)
                        {
                            serializer = (IManifestSerializer) Activator.CreateInstance(manifestAttr.Serializer);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid serializer was specified for given manifest");
                        }
                    }

                    serializer.Initialize(this);
                }

                _serializers[type] = serializer;

                return serializer;
            }
        }

        /**********************************************************************************/

        public CrateStorage ConvertFromProxy(CrateStorageDTO rawStorage)
        {
            var storage = new CrateStorage();

            if (rawStorage != null && rawStorage.Crates != null)
            {
                foreach (var crateDto in rawStorage.Crates)
                {
                    storage.Add(ConvertFromProxy(crateDto));
                }
            }

            return storage;
        }

        /**********************************************************************************/

        public CrateStorageDTO ConvertToProxy(CrateStorage storage)
        {
            var storageSerializationProxy = new CrateStorageDTO
            {
                Crates = new CrateDTO[storage.Count]
            };

            int id = 0;

            foreach (var crate in storage)
            {
                storageSerializationProxy.Crates[id] = ConvertToProxy(crate);
                id ++;
            }

            return storageSerializationProxy;
        }

        /**********************************************************************************/

        public Crate ConvertFromProxy(CrateDTO proxy)
        {
            var manifestType = new CrateManifestType(proxy.ManifestType, proxy.ManifestId);
            IManifestSerializer serializer = GetSerializer(manifestType);
            Crate crate;

            if (serializer != null)
            {
                if (proxy.Contents != null)
                {
                    crate = Crate.FromContent(serializer.Deserialize(proxy.Contents), proxy.Id);
                }
                else
                {
                    crate = new Crate(manifestType, proxy.Id);
                }
            }
            else
            {
                crate = Crate.FromJson(manifestType, proxy.Id, proxy.Contents);
            }

            crate.Label = proxy.Label;

            return crate;
        }

        /**********************************************************************************/

        public CrateDTO ConvertToProxy(Crate crate)
        {
            IManifestSerializer serializer = GetSerializer(crate.ManifestType);
            CrateDTO crateDto = new CrateDTO
            {
                Id = crate.Id,
                Label = crate.Label,
                ManifestId = crate.ManifestType.Id,
                ManifestType = crate.ManifestType.Type,
            };

            if (serializer != null)
            {
                crateDto.Contents = serializer.Serialize(crate.Get<object>());
            }
            else
            {
                crateDto.Contents = crate.GetRaw();
            }

            return crateDto;
        }

        /**********************************************************************************/
    }
}
