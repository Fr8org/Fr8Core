using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Core.Interfaces
{
    public interface IProcess
    {
        string GetEnvelopeIdFromXml(string xmlPayload);
        IEnumerable<ProcessDO> GetProcessListForUser(string userId);
        void HandleDocusignNotification(string userId, string xmlPayload);
    }
}
