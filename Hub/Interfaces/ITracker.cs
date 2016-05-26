using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface ITracker
    {
        void Identify(String userId);
        void Identify(Fr8AccountDO fr8AccountDO);
        void Track(Fr8AccountDO fr8AccountDO, String eventName, String action, Dictionary<String, object> properties = null);
        void Track(Fr8AccountDO fr8AccountDO, String eventName, Dictionary<String, object> properties = null);
        void Track(IUnitOfWork uow, string userId, string eventName, Segment.Model.Properties properties);
        void Track(string eventName, Segment.Model.Properties properties);
        void Track(string userId, string eventName, Segment.Model.Properties properties);
    }
}
