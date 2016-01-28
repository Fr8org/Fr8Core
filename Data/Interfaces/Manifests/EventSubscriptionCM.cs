using System.Collections.Generic;

namespace Data.Interfaces.Manifests
{
    public class EventSubscriptionCM : Manifest
    {
        public List<string> Subscriptions { get; set; }

        public EventSubscriptionCM()
            : base(Constants.MT.StandardEventSubscription)
        { 
        }

    }
}