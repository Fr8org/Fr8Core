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

        public IQueryable<ProcessTemplateDO> GetForUser(string userId, int? id = null)
        {
            return UnitOfWork
                .ProcessTemplateRepository
                .GetQuery()
                .Where(pt => pt.UserId == userId && (id != null && pt.Id == id));
        }


        public int CreateOrUpdate(ProcessTemplateDO ptdo)
        {
            var creating = ptdo.Id == 0;
            if (creating)
            {
                UnitOfWork.ProcessTemplateRepository.Add(ptdo);
            }
            else
            {
                var processTemplate = UnitOfWork.ProcessTemplateRepository.GetByKey(ptdo.Id);

                if (processTemplate == null)
                    throw new EntityNotFoundException();

                processTemplate.Name = ptdo.Name;
                processTemplate.Description = ptdo.Description;
            }
            UnitOfWork.SaveChanges();
            return ptdo.Id;
        }

        public void Delete(int id)
        {
            var processTemplate = UnitOfWork.ProcessTemplateRepository.GetByKey(id);
            if (processTemplate == null)
            {
                throw new EntityNotFoundException<ProcessTemplateDO>(id);
            }
            UnitOfWork.ProcessTemplateRepository.Remove(processTemplate);
            UnitOfWork.SaveChanges();

        }


    }

    public interface IProcessTemplateRepository : IGenericRepository<ProcessTemplateDO>
    {
        IQueryable<ProcessTemplateDO> GetForUser(string userId, int? id = null);

        int CreateOrUpdate(ProcessTemplateDO ptdo);
        void Delete(int id);

    }
}