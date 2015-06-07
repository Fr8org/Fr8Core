using Data.Interfaces;
using Data.States.Templates;

namespace Data.Repositories
{
    public class EmailStatusRepository : GenericRepository<_EmailStatusTemplate>, IEmailDOStatusRepository
    {
        public EmailStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }
    public interface IEmailDOStatusRepository : IGenericRepository<_EmailStatusTemplate>
    {

    }
}
