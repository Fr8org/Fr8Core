using System;
using Data.Entities;
using Data.States;
using BookingRequestState = Data.States.BookingRequestState;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public BookingRequestDO TestBookingRequest1()
        {
            var user = TestUser1();
            
            var curBookingRequestDO = new BookingRequestDO
                                          {
                                              Id = 1,
                                              From = TestEmailAddress1(),
                                              Subject = "Booking request subject",
                                              HTMLText = "Booking request text",
                                              EmailStatus = EmailState.Unprocessed,
                                              DateReceived = DateTimeOffset.UtcNow,
                                              State = BookingRequestState.AwaitingClient,
                                              Customer = user
                                          };
            user.EmailAddress.SentEmails.Add(curBookingRequestDO);
            return curBookingRequestDO;
        }

        public BookingRequestDO TestBookingRequest2()
        {
            var user = TestUser1();
            var curBookingRequestDO = new BookingRequestDO
            {
                Id = 2,
                CreateDate = DateTimeOffset.UtcNow,
                From = TestEmailAddress1(),
                Subject = "Booking request subject",
                HTMLText = "Booking request text",
                EmailStatus = EmailState.Unprocessed,
                DateReceived = DateTimeOffset.UtcNow,
                State = BookingRequestState.AwaitingClient,
                Customer = user
            };
            user.EmailAddress.SentEmails.Add(curBookingRequestDO);
            return curBookingRequestDO;
        }

    }
}
