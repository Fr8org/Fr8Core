using Data.Entities;
using Data.Interfaces;
using Data.Validations;
using FluentValidation;

namespace Data.Repositories
{


    public class EventRepository : GenericRepository<EventDO>, IEventRepository
    {
        private EventValidator _curValidator;

        internal EventRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new EventValidator();
            
        }

        public override void Add(EventDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }


    public interface IEventRepository : IGenericRepository<EventDO>
    {

    }
}
