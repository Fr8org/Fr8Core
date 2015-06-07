using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;
using StructureMap;

namespace KwasantCore.Services
{
    class ExpectedResponse : IExpectedResponse
    {
        private readonly IBookingRequest _br;

        public ExpectedResponse()
        {
            _br = ObjectFactory.GetInstance<IBookingRequest>();
        }

        public void MarkAsStale(IUnitOfWork uow, ExpectedResponseDO expectedResponseDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (expectedResponseDO == null)
                throw new ArgumentNullException("expectedResponseDO");

            expectedResponseDO.Status = ExpectedResponseStatus.Stale;

            BookingRequestDO bookingRequestDO = null;
            if (string.Equals(expectedResponseDO.AssociatedObjectType, "BookingRequest", StringComparison.Ordinal))
            {
                bookingRequestDO = uow.BookingRequestRepository.GetByKey(expectedResponseDO.AssociatedObjectID);
            }
            else if (string.Equals(expectedResponseDO.AssociatedObjectType, "Negotiation", StringComparison.Ordinal))
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(expectedResponseDO.AssociatedObjectID);
                if (negotiationDO != null)
                    bookingRequestDO = negotiationDO.BookingRequest;
            }
            if (bookingRequestDO != null)
            {
                _br.Reactivate(uow, bookingRequestDO);
            }
            uow.SaveChanges();
            AlertManager.AttendeeUnresponsivenessThresholdReached(expectedResponseDO.Id);
        }
    }
}
