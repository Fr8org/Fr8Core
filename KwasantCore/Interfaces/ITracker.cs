using System;
using System.Collections.Generic;
using Data.Entities;

namespace KwasantCore.Interfaces
{
    public interface ITracker
    {
        void Identify(String userID);
        void Identify(UserDO userDO);
        void Track(UserDO userDO, String eventName, String action, Dictionary<String, object> properties = null);
        void Track(UserDO userDO, String eventName, Dictionary<String, object> properties = null);
    }
}
