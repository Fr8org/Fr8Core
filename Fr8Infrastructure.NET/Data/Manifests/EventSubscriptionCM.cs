using System.Collections.Generic;
using fr8.Infrastructure.Data.Constants;

namespace fr8.Infrastructure.Data.Manifests
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