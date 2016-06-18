using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class TerminalRegistrationRepository : GenericRepository<TerminalRegistrationDO>
    {
        public TerminalRegistrationRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
