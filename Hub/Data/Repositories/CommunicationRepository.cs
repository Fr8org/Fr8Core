using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class CommunicationConfigurationRepository : GenericRepository<CommunicationConfigurationDO>, ICommunicationConfigurationRepository
    {
        internal CommunicationConfigurationRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface ICommunicationConfigurationRepository : IGenericRepository<CommunicationConfigurationDO>
    {

    }
}
