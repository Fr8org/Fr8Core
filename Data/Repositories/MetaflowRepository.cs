using Data.Interfaces;
using Data.Entities;
using Data.Validations;
using FluentValidation;

namespace Data.Repositories
{
    public class SlipRepository : GenericRepository<SlipDO>, ISlipRepository
    {
        private IncidentValidator _curValidator;
        internal SlipRepository(IUnitOfWork uow)
            : base(uow)
        {
            _curValidator = new IncidentValidator();
        }

       


    }
    public interface ISlipRepository : IGenericRepository<SlipDO>
    {
    }



}
