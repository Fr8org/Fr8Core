using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ProcessNodeTemplateRepository : GenericRepository<ProcessNodeTemplateDO>, IProcessNodeTemplateRepository
    {
        internal ProcessNodeTemplateRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IProcessNodeTemplateRepository : IGenericRepository<ProcessNodeTemplateDO>
    {
    }
}