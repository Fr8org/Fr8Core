using System;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;
using Utilities;

namespace KwasantWeb.NotificationQueues
{
    public class NewBookingRequestResponseQueue : SharedNotificationQueue<NewBookingRequestResponseData>
    {
        public NewBookingRequestResponseQueue()
        {
            AlertManager.AlertResponseReceived +=
                (bookingRequestID, bookerID, customerID) =>
                {
                    if (!String.IsNullOrWhiteSpace(bookerID))
                    {
                        AppendUpdate(new NewBookingRequestResponseData
                        {
                            UserID = bookerID,
                            BookingRequestID = bookingRequestID
                        });
                    }
                };
        }

        protected override TimeSpan ExpireUpdateAfter
        {
            get { return TimeSpan.FromSeconds(30); }
        }

        protected override void ObjectExpired(NewBookingRequestResponseData item)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDo = uow.UserRepository.GetByKey(item.UserID);
                var em = new Email();
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                const string message = @"Dear {0},<br/>
A booking request has recieved a new response.<br/>
Click <a href='{1}'>here</a> to check it out.";

                var formattedMessage = string.Format(message,
                    userDo.UserName,
                    Server.ServerUrl + "Dashboard/Index?id=" + item.BookingRequestID
                    );
                var emailDO = em.GenerateBasicMessage(uow, "A booking request has recieved a new response", formattedMessage, fromAddress, userDo.EmailAddress.Address);
                uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
                uow.SaveChanges();
            }
        }
    }

    public class NewBookingRequestResponseData : IUserUpdateData
    {
        public string UserID { get; set; }
        public int BookingRequestID { get; set; }
    }
}