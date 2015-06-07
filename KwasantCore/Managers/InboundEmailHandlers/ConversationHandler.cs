using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace KwasantCore.Managers.InboundEmailHandlers
{
    class ConversationHandler : IInboundEmailHandler
    {
        public bool TryHandle(MailMessage message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var headers = message.Headers.AllKeys.ToDictionary(header => header, header => message.Headers[header]);
                var existingBookingRequest = Conversation.Match(uow, headers, message.To, message.Subject, message.From.Address);
                if (existingBookingRequest != null)
                {
                    var createdEmailDO = Conversation.AddEmail(uow, message, existingBookingRequest);

                    var br = new BookingRequest();
                    var fromUser = uow.UserRepository.GetOrCreateUser(createdEmailDO.From);
                    br.AcknowledgeResponseToBookingRequest(uow, existingBookingRequest, createdEmailDO, fromUser.Id);

                    uow.SaveChanges();
                    return true;
                }
            }
            return false;
        }
    }
}
