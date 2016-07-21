using System.Collections.Generic;

namespace Fr8Data.Manifests
{
    public class EventSubscriptionCM : Manifest
    {
        public List<string> Subscriptions { get; set; }

        public string Manufacturer { get; set; }

        public EventSubscriptionCM()
            : base(Constants.MT.StandardEventSubscription)
        { 
        }

    }
}