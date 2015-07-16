using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;

namespace DockyardTest.Entities
{
	[ TestFixture ]
	public class EnvelopeTests: BaseTest
	{
		[ Test ]
		[ Category( "Envelope" ) ]
		public void Envelope_Change_Status()
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				uow.EnvelopeRepository.Add( new EnvelopeDO { Id = 1, Status = "Created" } );
				uow.SaveChanges();
			}
		}
	}
}