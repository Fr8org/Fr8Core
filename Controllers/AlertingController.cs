using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using StructureMap;
using Data.Interfaces;
using Hub.Managers;
using Web.NotificationQueues;

namespace Web.Controllers
{
    [DockyardAuthorize]
    public class AlertingController : Controller
    {
        [HttpPost]
        public ActionResult RegisterInterestInPageUpdates(string eventName, int objectID)
        {
            if (String.IsNullOrEmpty(eventName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var guid = Guid.NewGuid().ToString();

            if (Session[guid] == null)
            {
                var queue = PersonalNotificationQueues.GetQueueByName(eventName);
                queue.ObjectID = objectID;

                Session[guid] = queue;
            } 
            
            return Json(guid);
        }

        [HttpPost]
        public ActionResult RequestUpdate(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as IPersonalNotificationQueue;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return Json(queue.GetUpdates());
        }

        [HttpPost]
        public ActionResult RegisterInterestInUserUpdates(string eventName)
        {
            if (String.IsNullOrEmpty(eventName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var guid = Guid.NewGuid().ToString();
            var queue = (ISharedNotificationQueue<IUserUpdateData>)SharedNotificationQueues.GetQueueByName(eventName);
            queue.RegisterInterest(guid);
            Session[guid] = queue;

            return Json(guid);
        }

        [HttpPost]
        public ActionResult RequestUpdateForUser(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as ISharedNotificationQueue<IUserUpdateData>;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return Json(queue.GetUpdates(guid, i => i.UserID == this.GetUserId()));
        }

        [HttpPost]
        public ActionResult RegisterInterestInRoleUpdates(string eventName)
        {
            if (String.IsNullOrEmpty(eventName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var guid = Guid.NewGuid().ToString();
            var queue = (ISharedNotificationQueue<IRoleUpdateData>)SharedNotificationQueues.GetQueueByName(eventName);
            queue.RegisterInterest(guid);
            Session[guid] = queue;

            return Json(guid);
        }
        
        [HttpPost]
        public ActionResult RequestUpdateForRole(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as ISharedNotificationQueue<IRoleUpdateData>;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var roles = this.GetRoleNames();
            return Json(queue.GetUpdates(guid, i => i.RoleNames.Any(roles.Contains)));
        }
    }
}