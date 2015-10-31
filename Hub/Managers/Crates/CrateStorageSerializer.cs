using System;
using System.Collections.Generic;
using System.Reflection;
using Data.Crates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hub.Managers.Crates
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

        public CrateStorage Load(JToken serializedStorage)
        {
            if (serializedStorage == null)
            {
                return new CrateStorage();
            }

            return Load(serializedStorage.ToObject<CrateStorageSerializationProxy>());
        }

        /**********************************************************************************/

        public CrateStorage Load(string serializedStorage)
        {
            var rawStorage = JsonConvert.DeserializeObject<CrateStorageSerializationProxy>(serializedStorage);
            return Load(rawStorage);
        }


        /**********************************************************************************/

        private CrateStorage Load(CrateStorageSerializationProxy rawStorage)
        {
            var storage = new CrateStorage();

            if (rawStorage.Crates != null)
            {
                foreach (var crateDto in rawStorage.Crates)
                {
                    var manifestType = new CrateManifestType(crateDto.ManifestType, crateDto.ManifestId);
                    IManifestSerializer serializer = GetSerializer(manifestType);
                    Crate crate;

                    if (serializer != null)
                    {
                        crate = Crate.FromContent(serializer.Deserialize(crateDto.Contents), crateDto.Id);
                    }
                    else
                    {
                        crate = Crate.FromJson(manifestType, crateDto.Id, crateDto.Contents);
                    }

                    crate.Label = crateDto.Label;

                    storage.Add(crate);
                }
            }

            return storage;
        }

        /**********************************************************************************/

        private CrateStorageSerializationProxy ConvertToProxy(CrateStorage storage)
        {
            var storageSerializationProxy = new CrateStorageSerializationProxy
            {
                Crates = new List<CrateSerializationProxy>()
            };

            foreach (var crate in storage)
            {
                IManifestSerializer serializer = GetSerializer(crate.ManifestType);
                CrateSerializationProxy crateSerializationProxy = new CrateSerializationProxy
                {
                    Id = crate.Id,
                    Label = crate.Label,
                    ManifestId = crate.ManifestType.Id,
                    ManifestType = crate.ManifestType.Type,
                };

                if (serializer != null)
                {
                    crateSerializationProxy.Contents = serializer.Serialize(crate.Get<object>());
                }
                else
                {
                    crateSerializationProxy.Contents = crate.GetRaw();
                }

                storageSerializationProxy.Crates.Add(crateSerializationProxy);
            }

            return storageSerializationProxy;
        }

        /**********************************************************************************/

        public string SaveToString(CrateStorage storage)
        {
            var storageSerializationProxy = ConvertToProxy(storage);
            return JsonConvert.SerializeObject(storageSerializationProxy, Formatting.Indented);
        }

        /**********************************************************************************/

        public JToken SaveToJson(CrateStorage storage)
        {
            var storageSerializationProxy = ConvertToProxy(storage);
            return JToken.FromObject(storageSerializationProxy);
        }

        /**********************************************************************************/
    }
}
