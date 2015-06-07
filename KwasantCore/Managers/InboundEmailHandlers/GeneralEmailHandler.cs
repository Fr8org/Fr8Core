using System.Net.Mail;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace KwasantCore.Managers.InboundEmailHandlers
{
    class GeneralEmailHandler : IInboundEmailHandler
    {
        public bool TryHandle(MailMessage message)
        {
            BookingRequest.ProcessNewBR(message);
            return true;
        }
    }
}