using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data.Entities;
using Fr8.Infrastructure.Data.Helpers;

namespace Data.Repositories
{
    internal class AuthorizationTokenChangeTracker
    {
        private readonly AuthorizationTokenDO _tokenInstance;
        private AuthorizationTokenDO _propertiesTrackingReference;
        public List<IMemberAccessor> Changes;

        public EntityState State
        {
            get;
            set;
        }

        public bool IsSecureDataChanged => _propertiesTrackingReference.Token != _tokenInstance.Token;

        public bool HasChanges => Changes?.Count > 0;

        public AuthorizationTokenDO ActualValue => _tokenInstance;

        public AuthorizationTokenChangeTracker( AuthorizationTokenDO tokenInstance, EntityState state)
        {
            State = state;
            _tokenInstance = tokenInstance;
            _propertiesTrackingReference = _tokenInstance.Clone();
        }

        public void InjectSecretData(string secret)
        {
            _tokenInstance.Token = secret;
            _propertiesTrackingReference.Token = secret;
        }

        public void ResetChanges()
        {
            Changes = null;
            State = EntityState.Modified;
            _propertiesTrackingReference = _tokenInstance.Clone();
        }
        
        public void DetectChanges()
        {
            Changes = new List<IMemberAccessor>();

            if (State == EntityState.Modified)
            {
                foreach (var memberAccessor in AuthorizationTokenDO.Members.Where(x => x.CanRead && x.CanWrite))
                {
                    if (!Equals(memberAccessor.GetValue(this._tokenInstance), memberAccessor.GetValue(_propertiesTrackingReference)))
                    {
                        Changes.Add(memberAccessor);
                    }
                }
            }
        }
    }
}