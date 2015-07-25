using System.Collections.Generic;
using System.Linq;

using Core.Services;

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
			const EnvelopeDO.EnvelopeState newStatus = EnvelopeDO.EnvelopeState.Created;
			const EnvelopeDO.EnvelopeState updatedStatus = EnvelopeDO.EnvelopeState.Delivered;
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				uow.EnvelopeRepository.Add( new EnvelopeDO { Id = 1, Status = newStatus, DocusignEnvelopeId = "23" } );
				uow.SaveChanges();

				var createdEnvelope = uow.EnvelopeRepository.GetQuery().FirstOrDefault();
				Assert.NotNull( createdEnvelope );
				Assert.AreEqual( newStatus, createdEnvelope.Status );

				createdEnvelope.Status = updatedStatus;
				uow.SaveChanges();

				var updatedEnvelope = uow.EnvelopeRepository.GetQuery().FirstOrDefault();
				Assert.NotNull( updatedEnvelope );
				Assert.AreEqual( updatedStatus, updatedEnvelope.Status );
			}
		}

        [Test]
        [Category("Envelope")]
        public void Envelope_Can_Normalize_EnvelopeData()
	    {
            //TODO orkan:
            /*
             * a) programmatically create an Envelope in DocuSign in the developer sandbox account
             * b) populate it with some Tabs with values. Example "Amount" is a text field with value "45".
             * c) call Envelope#GetEnvelopeData
             * d) Verify that the returned List matches expectations.
             */

            const EnvelopeDO.EnvelopeState newStatus = EnvelopeDO.EnvelopeState.Created;
            EnvelopeDO envelopeDo = new EnvelopeDO {Id = 1, Status = newStatus, DocusignEnvelopeId = "23"};

            List<EnvelopeData> envelopeDatas = Envelope.GetEnvelopeData(envelopeDo);
            Assert.IsNotNull(envelopeDatas);
            Assert.IsTrue(envelopeDatas.Count > 0);
	    }
	}
}