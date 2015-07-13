using System.Net.Mail;
using Data.Interfaces;
using Core.Services;
using StructureMap;

namespace Core.Managers.InboundEmailHandlers
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