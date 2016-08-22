using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    /// <summary>
    /// Repository to work with TerminalDO entities.
    /// </summary>
    public class TerminalRepository : GenericRepository<TerminalDO>, ITerminalRepository
    {
        public TerminalRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    /// <summary>
    /// Repository interface to work with TerminalDO entities.
    /// </summary>
    public interface ITerminalRepository : IGenericRepository<TerminalDO>
    {
    }
}
