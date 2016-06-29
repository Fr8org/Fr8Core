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
//When a user visits Fr8, we try three ways to get their ID:
//1) If they're already logged in, we use their userID from the database
//2) If they're not logged in, we check their userID from a cookie we set (sessionID)
//3) If they don't have a cookie, we generate a new ID based on ASP's session
//When an unknown user performs actions, we log it under their ID taken from above (usually from the ASP session ID). If they then sign in, we push an alias between their previous session ID, and their new logged-in ID.
//This means, the following workflow:
//Anonymous user visits fr8.com
//Anonymous user plays the video
//Anonymous user signs in as 'rjrudman@gmail.com'.
//When viewing the profile for 'rjrudman@gmail.com' - the first two actions are properly retrieved.
//Also - when submitting the 'Try it out' form, we link them to the user which is generated for that email. 
//Although it's not perfect (they can enter any email) - it helps give us an idea of who's who. 
//From then on, all actions they've done in the past and future will be aggregated to the submitted email 
//(as well as any other accounts they register, assuming cookies aren't cleared, which we can't do anything about. 
//Aggregation only works for the most recently used account, however).
//Still working through updating our templated emails. I've taken advantage of the token authorization, which automatically logs them in when clicking on it - so the tracking works better. 
//This also works regardless of them having cookies or not
//The basic work is now done - to add more tracking, we just need to add more analytics.track('someEvent') - the rest of the identify and aliasing should be done automatically.
//Server side, we have the following methods:
//void Identify(String userID);
//void Identify(DockyardAccountDO DockyardAccountDO);
//void Track(DockyardAccountDO DockyardAccountDO, String eventName, String action, Dictionary<String, object> properties = null);
//void Track(DockyardAccountDO DockyardAccountDO, String eventName, Dictionary<String, object> properties = null);
//Example:
//ObjectFactory.GetInstance<ISegmentIO>().Track(bookingRequestDO.DockYardAccount, "BookingRequest", "Submit", new Dictionary<string, object> "BookingRequestId", bookingRequestDO.Id);
//They also help to push changes to a user (ie, name change, email change, etc, etc). Already setup to be mockable if needed
//namespace Core.Managers.APIManagers.Packagers.SegmentIO
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
                    { "AnonymousId", fr8AccountDO.Id }
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
            Analytics.Client.Identify(fr8AccountDO.Id,GetProperties(fr8AccountDO));
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
        public void Track(string eventName, Segment.Model.Properties properties)
        {
            if (Analytics.Client == null)
                return;
            Analytics.Client.Track(null, eventName, properties);
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
