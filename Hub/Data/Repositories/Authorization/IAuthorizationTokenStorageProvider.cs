using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Fr8.Infrastructure.Data.Helpers;

namespace Data.Repositories.Authorization
{
    public class AuthorizationTokenChanges
    {
        /**********************************************************************************/
        // Nested
        /**********************************************************************************/

        public class ChangedToken
        {
            public readonly List<IMemberAccessor> ChangedProperties;
            public readonly AuthorizationTokenDO Token;

            public ChangedToken(AuthorizationTokenDO token, List<IMemberAccessor> changedProperties)
            {
                Token = token;
                ChangedProperties = changedProperties;
            }
        }

        /**********************************************************************************/

        public readonly List<AuthorizationTokenDO> Insert = new List<AuthorizationTokenDO>();
        public readonly List<AuthorizationTokenDO> Delete = new List<AuthorizationTokenDO>();
        public readonly List<ChangedToken> Update = new List<ChangedToken>();

        public bool HasChanges
        {
            get { return Insert.Count > 0 || Delete.Count > 0 || Update.Count > 0; }
        }
    }


    public interface IAuthorizationTokenStorageProvider
    {
        void Update(AuthorizationTokenChanges changes);
        IQueryable<AuthorizationTokenDO> GetQuery();
        AuthorizationTokenDO GetByKey(Guid id);
    }
}
