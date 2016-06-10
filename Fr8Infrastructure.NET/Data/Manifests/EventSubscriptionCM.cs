using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class EventSubscriptionCM : Manifest
    {
        public List<string> Subscriptions { get; set; }

        public string Manufacturer { get; set; }

        public EventSubscriptionCM()
            : base(MT.StandardEventSubscription)
        { 
        }

    }
}