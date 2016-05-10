using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.Manifests;
using Newtonsoft.Json.Linq;

namespace Fr8Data.DataTransferObjects
{
    [CrateManifestSerializer(typeof(EventSerializer))]
    public class EventCM : Manifest
    {
        public string EventName { get; set; }

        public string PalletId { get; set; }

        public ICrateStorage CrateStorage { get; set; }

        public EventCM() 
            : base(MT.EventOrIncidentReport)
        {
            CrateStorage = new CrateStorage();
        }
    }

    public class LoggingDataCm : Manifest
    {
        public string ObjectId { get; set; }

        public string Fr8UserId { get; set; }

        public string Data { get; set; }

        public string PrimaryCategory { get; set; }

        public string SecondaryCategory { get; set; }

        public string Activity { get; set; }

        public LoggingDataCm()
            : base(MT.LoggingData)
        {
        }
    }
    
    public class EventSerializer : IManifestSerializer
    {
        public class EventCMSerializationProxy
        {
            public string EventName { get; set; }
            public string PalletId { get; set; }
            public CrateStorageDTO CrateStorage { get; set; }
        }

        private ICrateStorageSerializer _storageSerizlier;

        public void Initialize(ICrateStorageSerializer storageSerializer)
        {
            _storageSerizlier = storageSerializer;
        }

        public object Deserialize(JToken crateContent)
        {
            var proxy = crateContent.ToObject<EventCMSerializationProxy>();
            var storage = _storageSerizlier.ConvertFromDto(proxy.CrateStorage);

            return new EventCM
            {
                EventName = proxy.EventName,
                PalletId = proxy.PalletId,
                CrateStorage = storage
            };
        }

        public JToken Serialize(object content)
        {
            var e = (EventCM) content;
            
            var proxy = new EventCMSerializationProxy
            {
                EventName = e.EventName,
                PalletId = e.PalletId,
                CrateStorage = _storageSerizlier.ConvertToDto(e.CrateStorage)
            };

            return JToken.FromObject(proxy);
        }
    }
}
