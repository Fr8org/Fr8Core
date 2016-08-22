using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Repositories.Authorization;

namespace Data.Repositories
{
    class AuthorizationTokenRepositoryForTests : AuthorizationTokenRepositoryBase
    {
        private static readonly Dictionary<Guid, string>  TokenData = new Dictionary<Guid, string>();

        public AuthorizationTokenRepositoryForTests(IAuthorizationTokenStorageProvider storageProvider) 
            : base(storageProvider)
        {
        }

        protected override void ProcessSecureDataChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes)
        {
            foreach (var authorizationTokenDo in adds)
            {
                TokenData[authorizationTokenDo.Id] = authorizationTokenDo.Token;
            }

            foreach (var authorizationTokenDo in updates)
            {
                TokenData[authorizationTokenDo.Id] = authorizationTokenDo.Token;
            }

            foreach (var authorizationTokenDo in deletes)
            {
                TokenData.Remove(authorizationTokenDo.Id);
            }
        }

        protected override string QuerySecurePart(Guid id)
        {
            string data;

            TokenData.TryGetValue(id, out data);

            return data;
        }
    }
}
