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
                    storage.Add(ConvertFromProxy(crateDto));
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
                storageSerializationProxy.Crates.Add(ConvertToProxy(crate));
            }

            return storageSerializationProxy;
        }

        /**********************************************************************************/

        private Crate ConvertFromProxy(CrateSerializationProxy proxy)
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

        private CrateSerializationProxy ConvertToProxy(Crate crate)
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

            return crateSerializationProxy;
        }

        /**********************************************************************************/

        public JToken SaveToJson(Crate crate)
        {
            return JToken.FromObject(ConvertToProxy(crate));
        }

        /**********************************************************************************/

        public CrateSerializationProxy SaveToProxy(Crate crate)
        {
            return ConvertToProxy(crate);
        }
        
        /**********************************************************************************/

        public string SaveToString(Crate crate)
        {
            return JsonConvert.SerializeObject(ConvertToProxy(crate));
        }

        /**********************************************************************************/

        public Crate LoadCrate(string crate)
        {
            return ConvertFromProxy(JsonConvert.DeserializeObject<CrateSerializationProxy>(crate));
        }

        /**********************************************************************************/

        public Crate LoadCrate(CrateSerializationProxy crate)
        {
            return ConvertFromProxy(crate);
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
