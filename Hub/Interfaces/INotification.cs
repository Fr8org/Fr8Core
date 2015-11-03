using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface INotification
    {
        bool IsInNotificationWindow(string startTimeConfigName, string endTimeConfigName);
        void Generate(string userId, string message, TimeSpan expiresIn = default(TimeSpan));
    }
}
