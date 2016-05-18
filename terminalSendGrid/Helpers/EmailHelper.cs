using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8Data.DataTransferObjects;
using terminalSendGrid.Infrastructure;

namespace terminalSendGrid.Helpers
{
    public static class EmailHelper
    {
        public static List<EmailAddressDTO> GetToRecipients(this EmailDTO email)
        {
            return email.Recipients.Where(r => r.EmailParticipantType == EmailParticipantType.To)
                .Select(x => x.EmailAddress)
                .ToList();
        }
    }
}