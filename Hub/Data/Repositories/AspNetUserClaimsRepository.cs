using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetUserClaimsRepository : GenericRepository<AspNetUserClaimDO>, IAspNetUserClaimsRepository
    {
        internal AspNetUserClaimsRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }

        public void CreateNewClaim(string userId, string claimType, string claimValue)
        {
            Add(new AspNetUserClaimDO()
            {
                UserId = userId,
                ClaimType = claimType,
                ClaimValue =  claimValue
            });
        }
    }

    public interface IAspNetUserClaimsRepository : IGenericRepository<AspNetUserClaimDO>
    {
        void CreateNewClaim(string userId, string claimType, string claimValue);
    }
}
