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

        public EventSubscriptionCM(string manufacturer, params string[] subscriptions)
               : base(MT.StandardEventSubscription)
        {
            Manufacturer = manufacturer;
            Subscriptions = new List<string>(subscriptions);
        }
        
        public EventSubscriptionCM(string manufacturer, IEnumerable<string> subscriptions)
              : base(MT.StandardEventSubscription)
        {
            Manufacturer = manufacturer;
            Subscriptions = new List<string>(subscriptions);
        }


        public void Add(string subscription)
        {
            if (Subscriptions == null)
            {
                Subscriptions = new List<string>();
            }

            Subscriptions.Add(subscription);
        }

        public void AddRange(params string[] subscriptions)
        {
            AddRange((IEnumerable<string>)subscriptions);
        }

        public void AddRange(IEnumerable<string> subscriptions)
        {
            if (Subscriptions == null)
            {
                Subscriptions = new List<string>();
            }

            Subscriptions.AddRange(subscriptions);
        }
    }
}