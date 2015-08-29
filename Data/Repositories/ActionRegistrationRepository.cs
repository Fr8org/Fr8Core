using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ActionRegistrationRepository : GenericRepository<ActionTemplateDO>, IActionRegistrationRepository
    {
        public ActionRegistrationRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ActionTemplateDO entity)
        {
            base.Add(entity);
        }
    }

    public interface IActionRegistrationRepository : IGenericRepository<ActionTemplateDO>
    {

    }
}