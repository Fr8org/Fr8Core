using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Segment.Model;

namespace Hub.Interfaces
{
    public interface ITracker
    {
        void Identify(Fr8AccountDO fr8AccountDO);
        void Registered(string anonimousId, Fr8AccountDO fr8AccountDO);
        void Track(Fr8AccountDO fr8AccountDO, String eventName, Dictionary<String, object> properties = null);
    }
}
