using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class FactRepository : GenericRepository<FactDO>, IFactRepository
    {
        internal FactRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IFactRepository : IGenericRepository<FactDO>
    {

    }
}