using System.Linq;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Data.Repositories
{
    public class ProcessTemplateRepository : GenericRepository<ProcessTemplateDO>, IProcessTemplateRepository
    {
        internal ProcessTemplateRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public ProcessTemplateDO GetForUser(int id, string userId)
        {
            return UnitOfWork.ProcessTemplateRepository.GetQuery().Where(
                pt => pt.Id == id && pt.UserId == userId).FirstOrDefault();
        }

        public IQueryable<ProcessTemplateDO> GetForUser(string userId)
        {
            return UnitOfWork.ProcessTemplateRepository.GetQuery().Where(
                pt => pt.UserId == userId);
        }
    }

    public interface IProcessTemplateRepository : IGenericRepository<ProcessTemplateDO>
    {
        ProcessTemplateDO GetForUser(int id, string userId);
        IQueryable<ProcessTemplateDO> GetForUser(string userId);
    }
}