using System;
using System.Collections.Generic;
using Data.Entities;

namespace Core.Interfaces
{
    public interface ITracker
    {
        void Identify(String userID);
        void Identify(DockyardAccountDO dockyardAccountDO);
        void Track(DockyardAccountDO dockyardAccountDO, String eventName, String action, Dictionary<String, object> properties = null);
        void Track(DockyardAccountDO dockyardAccountDO, String eventName, Dictionary<String, object> properties = null);
    }
}
