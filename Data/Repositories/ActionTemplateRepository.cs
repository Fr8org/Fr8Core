using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ActionTemplateRepository : GenericRepository<ActionTemplateDO>, IActionTemplateRepository
    {
        public ActionTemplateRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ActionTemplateDO entity)
        {
            base.Add(entity);
        }
    }

    public interface IActionTemplateRepository : IGenericRepository<ActionTemplateDO>
    {

    }
}