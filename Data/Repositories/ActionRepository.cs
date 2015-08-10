using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ActionRepository : GenericRepository<ActionDO>, IActionRepository
    {
        public ActionRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ActionDO entity)
        {
            base.Add(entity);
        }
    }

    public interface IActionRepository : IGenericRepository<ActionDO>
    {

    }
}