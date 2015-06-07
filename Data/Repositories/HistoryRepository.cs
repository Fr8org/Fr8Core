using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class HistoryRepository : GenericRepository<HistoryItemDO>, IHistoryRepository
    {
        internal HistoryRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IHistoryRepository : IGenericRepository<HistoryItemDO>
    {

    }
}