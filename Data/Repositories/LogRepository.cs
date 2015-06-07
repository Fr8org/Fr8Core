using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class LogRepository : GenericRepository<LogDO>, ILogRepository
    {
        internal LogRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface ILogRepository : IGenericRepository<LogDO>
    {

    }
}