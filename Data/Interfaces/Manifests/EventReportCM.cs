using Data.Constants;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.Manifests
{
    [CrateManifestSerializer(typeof(EventReportSerializer))]
    public class EventReportCM : Manifest
    {
        public string EventNames { get; set; }
        public string ContainerDoId { get; set; }
        public string ExternalAccountId { get; set; }
        public CrateStorage EventPayload { get; set; }

        public string Source { get; set; }

        public EventReportCM()
            : base(MT.StandardEventReport)
         {
            EventPayload = new CrateStorage();
            //EventPayload = new List<CrateDTO>();
        }
    }


    public class EventReportSerializer : IManifestSerializer
    {
        public class EventReportCMSerializationProxy
        {
            public string EventNames { get; set; }
            public string ContainerDoId { get; set; }
            public string ExternalAccountId { get; set; }
            public CrateStorageDTO EventPayload { get; set; }
            public string Source { get; set; }
        }

        private ICrateStorageSerializer _storageSerizlier;

        public void Initialize(ICrateStorageSerializer storageSerializer)
        {
            _storageSerizlier = storageSerializer;
        }

        public object Deserialize(JToken crateContent)
        {
            var proxy = crateContent.ToObject<EventReportCMSerializationProxy>();
            var storage = _storageSerizlier.ConvertFromDto(proxy.EventPayload);

            return new EventReportCM
            {
                EventNames = proxy.EventNames,
                ContainerDoId = proxy.ContainerDoId,
                ExternalAccountId = proxy.ExternalAccountId,
                Source = proxy.EventNames,
                EventPayload = storage
            };
        }

        public JToken Serialize(object content)
        {
            var e = (EventReportCM) content;
            
            var proxy = new EventReportCMSerializationProxy
            {
                EventNames = e.EventNames,
                ContainerDoId = e.ContainerDoId,
                ExternalAccountId = e.ExternalAccountId,
                Source = e.EventNames,
                EventPayload = _storageSerizlier.ConvertToDto(e.EventPayload)
            };

            return JToken.FromObject(proxy);
        }
    }
}
