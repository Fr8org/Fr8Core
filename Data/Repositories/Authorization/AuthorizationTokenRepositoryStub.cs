using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    class AuthorizationTokenRepositoryStub : AuthorizationTokenRepositoryBase
    {
        public AuthorizationTokenRepositoryStub(IUnitOfWork uow) : base(uow)
        {
        }

        protected override void ProcessChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes)
        {
            if (adds.Any() || updates.Any() || deletes.Any())
            {
                throw new InvalidOperationException("Can't update authorization tokens. AuthorizationTokenRepository was not configured.");
            }
        }

        protected override string QuerySecurePart(Guid id)
        {
            throw new InvalidOperationException("Can't read authorization tokens. AuthorizationTokenRepository was not configured.");
        }
    }
}
