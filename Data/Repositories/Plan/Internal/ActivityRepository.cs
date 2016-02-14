using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Validations;
using FluentValidation;
namespace Data.Repositories
{
    public class ActivityRepository : GenericRepository<ActivityDO>, IActivityRepository
    {
        private ActionValidator _curValidator;
        public ActivityRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new ActionValidator();
        }

        public new void Add(ActivityDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }

    public interface IActivityRepository : IGenericRepository<ActivityDO>
    {

    }
}