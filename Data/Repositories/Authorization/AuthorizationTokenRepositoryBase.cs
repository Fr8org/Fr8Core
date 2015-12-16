using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Newtonsoft.Json;
using Utilities;

namespace Data.Repositories
{
    public abstract class AuthorizationTokenRepositoryBase : GenericRepository<AuthorizationTokenDO>, IAuthorizationTokenRepository, ITrackingChangesRepository
    {
        /*********************************************************************************/
        // Declarations
        /*********************************************************************************/

        private readonly Dictionary<Guid, AuthorizationTokenChangeTracker> _changesTackers = new Dictionary<Guid, AuthorizationTokenChangeTracker>();
        private readonly List<AuthorizationTokenDO> _adds = new List<AuthorizationTokenDO>();
        private readonly List<AuthorizationTokenDO> _deletes = new List<AuthorizationTokenDO>();
        
        /*********************************************************************************/

        public Type EntityType
        {
            get {return typeof (AuthorizationTokenDO); }
        }

        /*********************************************************************************/
        // Functions
        /*********************************************************************************/
        
        protected AuthorizationTokenRepositoryBase(IUnitOfWork uow)
            : base(uow)
        {
        }

//        These methods are quite weird and are used only in tests
//        /*********************************************************************************/
//
//        public String GetAuthorizationTokenURL(String url, Fr8AccountDO dockyardAccountDO, String segmentEventName = null, Dictionary<String, Object> segmentTrackingProperties = null)
//        {
//            return GetAuthorizationTokenURL(url, dockyardAccountDO.Id, segmentEventName, segmentTrackingProperties);
//        }
//
//        /*********************************************************************************/
//
//        public String GetAuthorizationTokenURL(String url, String userID, String segmentEventName = null, Dictionary<String, Object> segmentTrackingProperties = null)
//        {
//            var token = GetAuthorizationToken(url, userID, segmentEventName, segmentTrackingProperties);
//
//            var responseUrl = String.Format("{0}tokenAuth?token={1}",
//                Server.ServerUrl,
//                token);
//
//            return responseUrl;
//        }
//
//        /*********************************************************************************/
//
//        private String GetAuthorizationToken(String url, String userID, String segmentEventName = null, Dictionary<String, Object> segmentTrackingProperties = null)
//        {
//            var newTokenLink = new AuthorizationTokenDO
//            {
//                RedirectURL = url,
//                UserID = userID,
//                ExpiresAt = DateTime.UtcNow.AddDays(10),
//                SegmentTrackingEventName = segmentEventName,
//                Terminal = new TerminalDO()
//                {
//                    Id = 0,
//                    Name = "",
//                    Endpoint = "",
//                    Version = "1",
//                    TerminalStatus = TerminalStatus.Active
//                }
//            };
//
//            if (segmentTrackingProperties != null)
//                newTokenLink.SegmentTrackingProperties = JsonConvert.SerializeObject(segmentTrackingProperties);
//
//            UnitOfWork.AuthorizationTokenRepository.Add(newTokenLink);
//            return newTokenLink.Id.ToString();
//        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindToken(string userId, int terminalId, int? state)
        {
            AuthorizationTokenDO token;

            if (state == null)
            {
                token = GetQuery().FirstOrDefault(x => x.UserID == userId && x.TerminalID == terminalId);
            }
            else
            {
                token = GetQuery().FirstOrDefault(x => x.UserID == userId && x.TerminalID == terminalId && x.AuthorizationTokenState == state);
            }
            
            return EnrichAndTrack(token);
        }

        /*********************************************************************************/

        public int Count()
        {
            return GetQuery().Count();
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenByExternalAccount(string externalAccountId)
        {
            return EnrichAndTrack(GetQuery().FirstOrDefault(x => x.ExternalAccountId == externalAccountId));
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenById(string id)
        {
            return EnrichAndTrack(GetQuery().FirstOrDefault(x => x.Id.ToString() == id));
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenByUserId(string userId)
        {
            return EnrichAndTrack(GetQuery().FirstOrDefault(x => x.UserID == userId));
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenByExternalState(string externalStateToken)
        {
            return EnrichAndTrack(GetQuery().FirstOrDefault(x => x.ExternalStateToken == externalStateToken));
        }

        /*********************************************************************************/

        private AuthorizationTokenDO EnrichAndTrack(AuthorizationTokenDO token)
        {
            if (token == null)
            {
                return null;
            }

            AuthorizationTokenChangeTracker changeTracker;

            if (!_changesTackers.TryGetValue(token.Id, out changeTracker))
            {
                token.Token = QuerySecurePart(token.Id);
                _changesTackers[token.Id] = new AuthorizationTokenChangeTracker(token.Token, token);
            }

            return token;
        }

        /*********************************************************************************/

        public void TrackAdds(IEnumerable<object> entities)
        {
            foreach (var entity in entities.OfType<AuthorizationTokenDO>())
            {
                _adds.Add(entity);
                _changesTackers.Remove(entity.Id);
            }
        }

        /*********************************************************************************/

        public void TrackDeletes(IEnumerable<object> entities)
        {
            foreach (var entity in entities.OfType<AuthorizationTokenDO>())
            {
                _deletes.Add(entity);
                _changesTackers.Remove(entity.Id);
            }
        }

        /*********************************************************************************/

        public void TrackUpdates(IEnumerable<object> entities)
        {
        }

        /*********************************************************************************/

        public void SaveChanges()
        {
            ProcessChanges(_adds, _changesTackers.Values.Where(x=>x.HasChanges).Select(x=>x.ActualValue), _deletes);
            
            foreach (var value in _changesTackers)
            {
                if (value.Value.HasChanges)
                {
                    value.Value.ResetChanges();
                }
            }

            _adds.Clear();
            _deletes.Clear();
        }

        /*********************************************************************************/

        protected abstract void ProcessChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes);
        protected abstract string QuerySecurePart(Guid id);

        /*********************************************************************************/
    }
}