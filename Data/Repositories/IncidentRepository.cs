using Data.Interfaces;
using Data.Entities;
using Data.Validations;
using FluentValidation;

namespace Data.Repositories
{
    public class IncidentRepository : GenericRepository<IncidentDO>, IIncidentRepository
    {
        private IncidentValidator _curValidator;
        internal IncidentRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new IncidentValidator();
        }

        public override void Add(IncidentDO entity)
        {
            _curValidator.ValidateAndThrow(entity);
            base.Add(entity);
        }


    }
    public interface IIncidentRepository : IGenericRepository<IncidentDO>
    {
    }



}
