using Data.Entities;
using Data.Interfaces;
using Data.States;

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
            entity.ActionState = ActionState.Unstarted;
            base.Add(entity);
        }
    }

    public interface IActionRepository : IGenericRepository<ActionDO>
    {

    }
}