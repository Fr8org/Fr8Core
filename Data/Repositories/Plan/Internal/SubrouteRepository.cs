using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class SubrouteRepository : GenericRepository<SubrouteDO>, ISubrouteRepository
    {
        internal SubrouteRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    public interface ISubrouteRepository : IGenericRepository<SubrouteDO>
    {
    }
}