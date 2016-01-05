using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ProcessNodeRepository : GenericRepository<ProcessNodeDO>, IProcessNodeRepository
    {
        public ProcessNodeRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ProcessNodeDO entity)
        {
            base.Add(entity);
        }
    }

    public interface IProcessNodeRepository : IGenericRepository<ProcessNodeDO>
    {

    }
}