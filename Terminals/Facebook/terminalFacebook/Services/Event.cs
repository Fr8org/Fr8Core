using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Newtonsoft.Json;
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
                    UserId = entry.Uid
                };
                var eventReportContent = new EventReportCM
                {
                    EventNames = string.Join(",", fbEventCM.ChangedFields),
                    EventPayload = new CrateStorage(Crate.FromContent("Facebook user event", fbEventCM)),
                    Manufacturer = "Facebook",
                    ExternalAccountId = fbEventCM.UserId
                };
                ////prepare the event report
                var curEventReport = Crate.FromContent("Facebook user event", eventReportContent);
                eventList.Add(curEventReport);
            }
            return eventList;
        }
    }
}