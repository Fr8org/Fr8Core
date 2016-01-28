using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Validations;
using FluentValidation;
namespace Data.Repositories
{
    public class ActionRepository : GenericRepository<ActionDO>, IActionRepository
    {
        private ActionValidator _curValidator;
        public ActionRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new ActionValidator();
        }

        public new void Add(ActionDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }

    public interface IActionRepository : IGenericRepository<ActionDO>
    {

    }
}