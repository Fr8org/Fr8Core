using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ProcessTemplateRepository : GenericRepository<ProcessTemplateDO>, IProcessTemplateRepository
    {
        internal ProcessTemplateRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public IQueryable<ProcessTemplateDO> GetForUser(string userId, int? id = null)
        {
            return UnitOfWork
                .ProcessTemplateRepository
                .GetQuery()
                .Where(pt => pt.UserId == userId && (id != null && pt.Id == id));
        }



    }

    public interface IProcessTemplateRepository : IGenericRepository<ProcessTemplateDO>
    {
        IQueryable<ProcessTemplateDO> GetForUser(string userId, int? id = null);

    }
}