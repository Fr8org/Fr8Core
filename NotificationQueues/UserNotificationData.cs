using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KwasantWeb.NotificationQueues
{
    public class UserNotificationData : IUserUpdateData
    {
        public UserNotificationData(string userId, string message)
        {
            UserID = userId;
            Message = message;
        }

        public string Message { get; private set; }
        public string UserID { get; set; }
    }
}