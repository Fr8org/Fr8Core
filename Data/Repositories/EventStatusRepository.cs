using Data.Interfaces;
using Data.States.Templates;

namespace Data.Repositories
{
    public class EventStatusRepository : GenericRepository<_EventStatusTemplate>, IEventStatusRepository
    {
        public EventStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IEventStatusRepository : IGenericRepository<_EventStatusTemplate>
    {

    }
}


