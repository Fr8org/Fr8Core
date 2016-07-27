using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class HubSubscriptionCM : Manifest
    {
        [MtPrimaryKey]
        public string HubUrl { get; set; }

        public HubSubscriptionCM() 
            : base(MT.HubSubscription)
        {
        }

        public HubSubscriptionCM(string hubUrl)
            : base(MT.HubSubscription)
        {
            HubUrl = hubUrl;
        }
    }
}
