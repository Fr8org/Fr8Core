using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class RemoteServiceAuthDataRepository : GenericRepository<RemoteOAuthDataDo>, IRemoteCalendarAuthDataRepository
    {
        internal RemoteServiceAuthDataRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public RemoteOAuthDataDo GetOrCreate(string userId, string providerName)
        {
            var curUserAuthData = GetQuery().FirstOrDefault(ad => ad.Provider.Name == providerName && ad.UserID == userId);
            if (curUserAuthData == null)
            {
                var provider = UnitOfWork.RemoteServiceProviderRepository.GetByName(providerName);
                var user = UnitOfWork.UserRepository.GetByKey(userId);
                curUserAuthData = new RemoteOAuthDataDo
                                      {
                                          ProviderID = provider.Id,
                                          Provider = provider, 
                                          UserID = user.Id,
                                          User = user
                                      };
                Add(curUserAuthData);
            }
            return curUserAuthData;
        }
    }

    public interface IRemoteCalendarAuthDataRepository : IGenericRepository<RemoteOAuthDataDo>
    {
    }
}
