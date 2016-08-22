using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ActivityTemplateRepository : GenericRepository<ActivityTemplateDO>, IActivityTemplateRepository
    {
        public ActivityTemplateRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ActivityTemplateDO entity)
        {
            base.Add(entity);
        }
    }

    public interface IActivityTemplateRepository : IGenericRepository<ActivityTemplateDO>
    {

    }
}