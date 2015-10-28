using System;
using System.Collections.Generic;
using Data.Entities;

namespace Hub.Interfaces
{
    public interface ITracker
    {
        void Identify(String userID);
        void Identify(Fr8AccountDO dockyardAccountDO);
        void Track(Fr8AccountDO dockyardAccountDO, String eventName, String action, Dictionary<String, object> properties = null);
        void Track(Fr8AccountDO dockyardAccountDO, String eventName, Dictionary<String, object> properties = null);
    }
}
