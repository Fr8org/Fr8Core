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

        public override void Add(ProcessNodeTemplateDO entity)
        {
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                System.Diagnostics.Debugger.Launch();
            }

            base.Add(entity);
        }
    }

    public interface IProcessNodeTemplateRepository : IGenericRepository<ProcessNodeTemplateDO>
    {
    }
}