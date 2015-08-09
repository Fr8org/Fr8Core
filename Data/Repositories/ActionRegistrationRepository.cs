using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ActionRegistrationRepository : GenericRepository<ActionRegistrationDO>, IActionRegistrationRepository
    {
        public ActionRegistrationRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ActionRegistrationDO entity)
        {
            base.Add(entity);
        }
    }

    public interface IActionRegistrationRepository : IGenericRepository<ActionRegistrationDO>
    {

    }
}