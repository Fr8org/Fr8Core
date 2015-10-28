using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Hub.Interfaces;
using Hub.Managers.InboundEmailHandlers;

namespace Hub.Managers
{
    class IntakeManager : IIntakeManager
    {
        private readonly IInboundEmailHandler[] _handlers;

        public IntakeManager()
        {
            _handlers = new IInboundEmailHandler[]
                            {
                                new InvitationResponseHandler(),
                                //new ConversationHandler(), 
                                new GeneralEmailHandler()
                            };
        }

        public void AddEmail(MailMessage mailMessage)
        {
            var handlerIndex = 0;
            while (handlerIndex < _handlers.Length && !_handlers[handlerIndex].TryHandle(mailMessage))
            {
                handlerIndex++;
            }
            if (handlerIndex >= _handlers.Length)
                throw new ApplicationException("Message hasn't been processed by any handler.");
        }
    }
}
