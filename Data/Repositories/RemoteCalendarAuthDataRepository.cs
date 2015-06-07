using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class RemoteCalendarAuthDataRepository : GenericRepository<RemoteCalendarAuthDataDO>, IRemoteCalendarAuthDataRepository
    {
        internal RemoteCalendarAuthDataRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public RemoteCalendarAuthDataDO GetOrCreate(string userId, string providerName)
        {
            var curUserAuthData = GetQuery().FirstOrDefault(ad => ad.Provider.Name == providerName && ad.UserID == userId);
            if (curUserAuthData == null)
            {
                var provider = UnitOfWork.RemoteCalendarProviderRepository.GetByName(providerName);
                var user = UnitOfWork.UserRepository.GetByKey(userId);
                curUserAuthData = new RemoteCalendarAuthDataDO
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

    public interface IRemoteCalendarAuthDataRepository : IGenericRepository<RemoteCalendarAuthDataDO>
    {
    }
}
