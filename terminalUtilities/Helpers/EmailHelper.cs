using System.Collections.Generic;
using System.Linq;
using Fr8Data.DataTransferObjects;
using terminalUtilities.Infrastructure;

namespace terminalUtilities.Helpers
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