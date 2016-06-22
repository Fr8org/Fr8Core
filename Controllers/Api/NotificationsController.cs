using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using StructureMap;
using Hub.Infrastructure;
using HubWeb.Infrastructure_HubWeb;
using Data.Infrastructure.StructureMap;

namespace HubWeb.Controllers
{
    public class NotificationsController : Fr8BaseApiController
    {

        //TODO create an enum for different types of pusher events
        private const string PUSHER_EVENT_TERMINAL_NOTIFICATION = "fr8pusher_terminal_event";
        private IPusherNotifier _notification;
        private readonly ISecurityServices _security;

        public NotificationsController()
        {
            _notification = ObjectFactory.GetInstance<IPusherNotifier>();
        }

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public IHttpActionResult Post(TerminalNotificationDTO notificationMessage)
        {
            string userId;

            if (IsThisTerminalCall())
            {
                var user = GetUserTerminalOperatesOn();
                userId = user?.Id;
            }
            else
            {
                userId = _security.GetCurrentUser();
            }

            _notification.NotifyUser(notificationMessage, PUSHER_EVENT_TERMINAL_NOTIFICATION, userId);
            return Ok();
        }
    }
}