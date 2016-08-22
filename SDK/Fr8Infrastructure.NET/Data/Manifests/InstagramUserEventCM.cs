using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class InstagramUserEventCM : Manifest
    {
        public string MediaId { get; set; }
        public string UserId { get; set; }
        public string Time { get; set; }
        public string ChangedAspect { get; set; }
        public string SubscriptionId { get; set; }
        public InstagramUserEventCM(): base(MT.InstagramUserEvent)
        {

        }
    }
}
