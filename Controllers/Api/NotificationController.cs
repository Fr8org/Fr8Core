using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hangfire;
using Hub.Interfaces;
using Hub.Managers;
using HubWeb.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities.Interfaces;

namespace HubWeb.Controllers
{
    public class NotificationController : ApiController
    {
        private readonly IPusherNotifier _pusherNotifier;

        //TODO create an enum for different types of pusher events
        private const string PUSHER_EVENT_TERMINAL_NOTIFICATION = "fr8pusher_terminal_event";
        public NotificationController()
        {
            _pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();
        }

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post(TerminalNotificationDTO notificationMessage)
        {
            string pusherChannel = String.Format("fr8pusher_{0}", User.Identity.Name);
            _pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_TERMINAL_NOTIFICATION, notificationMessage);
            return Ok();
        }
    }
}