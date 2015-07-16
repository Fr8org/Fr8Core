using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
	public class EnvelopeRepository: GenericRepository< EnvelopeDO >, IEnvelopeRepository
	{
		public EnvelopeRepository( IUnitOfWork uow ): base( uow )
		{
		}

		public new void Add( EnvelopeDO entity )
		{
			base.Add( entity );
		}

		public void Update( EnvelopeDO entity )
		{
			var envelope = this.GetByKey( entity );
			envelope.Status = entity.Status;
			envelope.DocusignEnvelopeId = entity.DocusignEnvelopeId;
		}
	}

	public interface IEnvelopeRepository: IGenericRepository< EnvelopeDO >
	{

	}
}