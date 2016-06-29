using System;
using System.Collections.Generic;
using Segment;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using Hub.Services;
using Newtonsoft.Json.Schema;
using Segment.Model;

namespace Hub.Managers.APIManagers.Packagers.SegmentIO
{
    public class SegmentIO : ITracker
    {
        private readonly Fr8Account _fr8Account;

        public SegmentIO(Fr8Account fr8Account)
        {
            if (fr8Account == null)
            {
                throw new ArgumentNullException(nameof(fr8Account));
            }
            _fr8Account = fr8Account;
        }

        public void Identify(string userID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Identify(uow.UserRepository.GetByKey(userID));
            }
        }

        private Traits GetProperties(Fr8AccountDO fr8AccountDO)
        {
            return new Traits
            {
                {"First Name", fr8AccountDO.FirstName},
                {"Last Name", fr8AccountDO.LastName},
                {"Username", fr8AccountDO.UserName},
                {"Email", fr8AccountDO.EmailAddress.Address},
                {"Delegate Account", _fr8Account.GetMode(fr8AccountDO) == CommunicationMode.Delegate },
                {"Class", fr8AccountDO.Class }
            };
        }

        public void Alias(string anonimousId, Fr8AccountDO fr8AccountDO)
        {
            if (Analytics.Client == null)
                return;
            Analytics.Client.Alias(anonimousId, fr8AccountDO.Id);
            Analytics.Client.Flush();

            var userProperties = GetProperties(fr8AccountDO);
            Analytics.Client.Identify(fr8AccountDO.Id, userProperties);
            Analytics.Client.Track(fr8AccountDO.Id, "User Registered");
        }
        public void Identify(Fr8AccountDO fr8AccountDO)
        {
            if (Analytics.Client == null)
                return;
            var props = new Segment.Model.Properties();
            foreach (var prop in GetProperties(fr8AccountDO))
                props.Add(prop.Key, prop.Value);
            Analytics.Client.Identify(fr8AccountDO.Id, GetProperties(fr8AccountDO));
            Options mpCallOptions = new Options()
                .SetIntegration("all", false)
                .SetIntegration("Mixpanel", true)
                .SetContext(new Context() {
                    { "distinct_id", fr8AccountDO.Id }
                });
            Analytics.Client.Track(fr8AccountDO.Id, "User Logged In", props, mpCallOptions);
        }

        public void Track(Fr8AccountDO fr8AccountDO, string eventName, string action, Dictionary<string, object> properties = null)
        {
            if (Analytics.Client == null)
                return;
            if (properties == null)
                properties = new Dictionary<string, object>();
            properties["Activity Name"] = action;

            Track(fr8AccountDO, eventName, properties);
        }

        public void Track(Fr8AccountDO fr8AccountDO, string eventName, Dictionary<string, object> properties = null)
        {
            if (Analytics.Client == null)
                return;
            var props = new Segment.Model.Properties();
            foreach (var prop in GetProperties(fr8AccountDO))
                props.Add(prop.Key, prop.Value);

            if (properties != null)
            {
                foreach (var prop in properties)
                    props[prop.Key] = prop.Value;
            }
            Analytics.Client.Identify(fr8AccountDO.Id, GetProperties(fr8AccountDO));
            Analytics.Client.Track(fr8AccountDO.Id, eventName, props);
        }
        public void Track(IUnitOfWork uow, string userId, string eventName, Segment.Model.Properties properties)
        {
            if (Analytics.Client == null)
                return;
            Fr8AccountDO fr8AccountDO;
            using (uow) { fr8AccountDO = uow.UserRepository.GetByKey(userId); }
            var props = new Segment.Model.Properties();
            foreach (var prop in GetProperties(fr8AccountDO))
                props.Add(prop.Key, prop.Value);

            Analytics.Client.Track(fr8AccountDO.Id, eventName, props);
        }
        public void Track(string userId, string eventName, Dict properties)
        {
            if (Analytics.Client == null)
                return;
            var newProps = new Segment.Model.Properties();
            foreach (var prop in properties)
            {
                newProps.Add(prop.Key, prop.Value);
            }
            Analytics.Client.Track(userId, eventName, newProps);
            Analytics.Client.Flush();
        }
    }
}
