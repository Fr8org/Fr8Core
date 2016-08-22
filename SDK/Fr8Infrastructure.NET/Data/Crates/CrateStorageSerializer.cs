using System;
using System.Collections.Generic;
using System.Reflection;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Crates
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
        /// <summary>
        /// Register custom serializer for manifest type. 
        /// You don't have to call this method wile your manifest resides in Data project. 
        /// </summary>
        /// <param name="manifestType"></param>
        /// <param name="serializer"></param>
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
                    var manifestAttr = clrType.GetCustomAttribute<CrateManifestSerializerAttribute>();

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
        /// <summary>
        /// Convert CrateStorageDTO to CrateStorage
        /// </summary>
        /// <param name="rawStorage"></param>
        /// <returns></returns>
        public ICrateStorage ConvertFromDto(CrateStorageDTO rawStorage)
        {
            var storage = new CrateStorage();

            if (rawStorage != null && rawStorage.Crates != null)
            {
                foreach (var crateDto in rawStorage.Crates)
                {
                    storage.Add(ConvertFromDto(crateDto));
                }
            }

            return storage;
        }

        /**********************************************************************************/
        /// <summary>
        /// Convert CrateStorage to DTO
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public CrateStorageDTO ConvertToDto(ICrateStorage storage)
        {
            var storageSerializationProxy = new CrateStorageDTO
            {
                Crates = new CrateDTO[storage.Count]
            };

            int id = 0;

            foreach (var crate in storage)
            {
                storageSerializationProxy.Crates[id] = ConvertToDto(crate);
                id ++;
            }

            return storageSerializationProxy;
        }

        /**********************************************************************************/
        /// <summary>
        /// Convert DTO to Crate instance
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public Crate ConvertFromDto(CrateDTO proxy)
        {
            if (proxy == null)
            {
                return null;
            }

            var manifestType = new CrateManifestType(proxy.ManifestType, proxy.ManifestId);
            IManifestSerializer serializer = GetSerializer(manifestType);
            Crate crate;

            if (serializer != null)
            {
                var content = proxy.Contents != null ? serializer.Deserialize(proxy.Contents) : null;

                if (content != null)
                {
                    crate = Crate.FromContent(content, proxy.Id);
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
            crate.SourceActivityId = proxy.SourceActivityId;
            return crate;
        }

        /**********************************************************************************/
        /// <summary>
        /// Convert crate to DTO
        /// </summary>
        /// <param name="crate"></param>
        /// <returns></returns>
        public CrateDTO ConvertToDto(Crate crate)
        {
            if (crate == null)
            {
                return null;
            }

            IManifestSerializer serializer = GetSerializer(crate.ManifestType);
            CrateDTO crateDto = new CrateDTO
            {
                Id = crate.Id,
                Label = crate.Label,
                ManifestId = crate.ManifestType.Id,
                ManifestType = crate.ManifestType.Type,
                SourceActivityId = crate.SourceActivityId
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
