using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
	public class ActionListRepository: GenericRepository< ActionListDO >, IActionListRepository
	{
		public ActionListRepository( IUnitOfWork uow )
			: base( uow )
		{

		}

		public new void Add( ActionListDO entity )
		{
			base.Add( entity );
		}
	}

	public interface IActionListRepository: IGenericRepository< ActionListDO >
	{

	}
}