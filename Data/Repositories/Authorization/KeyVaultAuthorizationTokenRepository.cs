using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    class KeyVaultAuthorizationTokenRepository : AuthorizationTokenRepositoryBase
    {
        public KeyVaultAuthorizationTokenRepository(IUnitOfWork uow) 
            : base(uow)
        {
        }

        protected override void ProcessChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes)
        {
            throw new NotImplementedException();
        }

        protected override string QuerySecurePart(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
