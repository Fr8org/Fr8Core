using System.Net.Mail;
using StructureMap;
using Data.Interfaces;
using Hub.Services;

namespace Hub.Managers.InboundEmailHandlers
{
    class GeneralEmailHandler : IInboundEmailHandler
    {
        public bool TryHandle(MailMessage message)
        {
            //BookingRequest.ProcessNewBR(message);
            return true;
        }
    }
}