using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
	public class ExternalEventRegistrationRepository: GenericRepository< ExternalEventRegistrationDO >, IExternalEventRegistrationRepository
	{
		public ExternalEventRegistrationRepository( IUnitOfWork uow )
			: base( uow )
		{

		}

		public new void Add( ExternalEventRegistrationDO entity )
		{
			base.Add( entity );
		}
	}

	public interface IExternalEventRegistrationRepository: IGenericRepository< ExternalEventRegistrationDO >
	{

	}
}