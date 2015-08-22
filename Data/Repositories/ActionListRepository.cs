using Data.Entities;
using Data.Interfaces;
using Data.States;

namespace Data.Repositories
{
    public class ActionListRepository : GenericRepository<ActionListDO>, IActionListRepository
    {
        public ActionListRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ActionListDO entity)
        {
            entity.ActionListState = ActionListState.Unstarted;
            base.Add(entity);
        }
    }

    public interface IActionListRepository : IGenericRepository<ActionListDO>
    {

    }
}