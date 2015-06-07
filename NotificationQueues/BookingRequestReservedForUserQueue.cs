using System;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;
using Utilities;

namespace KwasantWeb.NotificationQueues
{
    public class BookingRequestReservedForUserQueue : SharedNotificationQueue<NewBookingRequestForUserQueueData>
    {
        public BookingRequestReservedForUserQueue() 
        {
            AlertManager.AlertBookingRequestReserved +=
                (bookingRequestID, bookerID) =>
                    AppendUpdate(new NewBookingRequestForUserQueueData
                    {
                        BookingRequestID = bookingRequestID,
                        UserID = bookerID,
                    });
        }

        protected override TimeSpan ExpireUpdateAfter
        {
            get { return TimeSpan.FromSeconds(30); }
        }

        protected override void ObjectExpired(NewBookingRequestForUserQueueData item)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDo = uow.UserRepository.GetByKey(item.UserID);
                var em = new Email();
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");


                const string message = @"Dear {0},<br/>
A booking request has been reserved for you.<br/>
Click <a href='{1}'>here</a> to check it out.";

                var formattedMessage = string.Format(message,
                    userDo.UserName,
                    Server.ServerUrl + "Dashboard/Index?id=" + item.BookingRequestID
                    );
                var emailDO = em.GenerateBasicMessage(uow, "Booking request was reserved for you", formattedMessage, fromAddress, userDo.EmailAddress.Address);
                uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
                uow.SaveChanges();
            }
        }
    }

    public class NewBookingRequestForUserQueueData : IUserUpdateData
    {
        public string UserID { get; set; }
        public int BookingRequestID { get; set; }
    }
}