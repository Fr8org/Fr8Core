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
    public class NotificationsController : Fr8BaseApiController
    {

        //TODO create an enum for different types of pusher events
        private const string PUSHER_EVENT_TERMINAL_NOTIFICATION = "fr8pusher_terminal_event";
        private IPusherNotifier _notification;

        public NotificationsController()
        {
            _notification = ObjectFactory.GetInstance<IPusherNotifier>();
        }

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public IHttpActionResult Post(TerminalNotificationDTO notificationMessage)
        {
            string userName;

            if (IsThisTerminalCall())
            {
                var user = GetUserTerminalOperatesOn();
                userName = user?.UserName;
            }
            else
            {
                userName = User.Identity.Name;
            }

            _notification.NotifyUser(notificationMessage, PUSHER_EVENT_TERMINAL_NOTIFICATION, userName);
            return Ok();
        }
    }
}