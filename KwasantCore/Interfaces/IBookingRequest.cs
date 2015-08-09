using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;

namespace KwasantCore.Interfaces
{
    public interface IBookingRequest 
    {
        void Process(IUnitOfWork uow, BookingRequestDO bookingRequest);

        List<object> GetAllByUserId(IBookingRequestDORepository curBookingRequestRepository, int start,
            int length, string userid);

        int GetBookingRequestsCount(IBookingRequestDORepository curBookingRequestRepository, string userid);
        string GetUserId(IBookingRequestDORepository curBookingRequestRepository, int bookingRequestId);
        object GetUnprocessed(IUnitOfWork uow);
        IEnumerable<object> GetRelatedItems(IUnitOfWork uow, int bookingRequestId);
        void Timeout(IUnitOfWork uow, BookingRequestDO bookingRequestDO);
        void ExtractEmailAddresses(IUnitOfWork uow, EventDO eventDO);
        object GetAllBookingRequests(IUnitOfWork uow);
        UserDO GetPreferredBooker(BookingRequestDO bookingRequestDO);
        String GetConversationThread(BookingRequestDO bookingRequestDO);
     
        void CheckOut(int bookingRequestId, string bookerId);
        void ReleaseBooker(int bookingRequestId);
        void Reactivate(IUnitOfWork uow, BookingRequestDO bookingRequestDO);
        void Reserve(IUnitOfWork uow, BookingRequestDO bookingRequestDO, UserDO booker);
        void ReservationTimeout(IUnitOfWork uow, BookingRequestDO bookingRequestDO);
    }

}