using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using terminalFacebook.Interfaces;
using terminalFacebook.Models;

namespace terminalFacebook.Services
{
    public class Event : IEvent
    {
        private readonly ICrateManager _crateManager;
        private readonly IFacebookIntegration _fbIntegration;
        private const string FEED_UPDATE = "feed";

        public Event(IFacebookIntegration fbIntegration, ICrateManager crateManager)
        {
            _crateManager = crateManager;
            _fbIntegration = fbIntegration;
        }

        public async Task<List<Crate>> ProcessUserEvents(IContainer container, string curExternalEventPayload)
        {
            var notification = JsonConvert.DeserializeObject<UserNotification>(curExternalEventPayload);
            if (notification.Object != "user")
            {
                throw new Exception("Unknown event source");
            }
            
            var eventList = new List<Crate>();
            foreach (var entry in notification.Entry)
            {
                var fbEventCM = new FacebookUserEventCM
                {
                    Id = entry.Id,
                    ChangedFields = entry.ChangedFields,
                    Time = entry.Time,
                    Uid = entry.Uid
                };
                
                eventList.Add(Crate.FromContent("Facebook user event", fbEventCM));
            }
            return eventList;
        }
        

        //private Crate ProcessEntry(Entry entry)
        //{
        //    foreach (var changedField in entry.ChangedFields)
        //    {
        //        switch (changedField)
        //        {
        //            case FEED_UPDATE:
                        
        //                break;
        //        }
        //    }
        //    ////prepare the event report
        //    var curEventReport = Crate.FromContent("Standard Event Report", null);
        //    return curEventReport;
        //}
    }
}