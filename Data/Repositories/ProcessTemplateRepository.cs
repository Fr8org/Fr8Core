using System.Linq;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ProcessTemplateRepository : GenericRepository<ProcessTemplateDO>, IProcessTemplateRepository
    {
        internal ProcessTemplateRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IProcessTemplateRepository : IGenericRepository<ProcessTemplateDO>
    {

    }
}