using System;
using Fr8.Infrastructure;

namespace HubWeb.Documentation.Swagger
{
    public class PhoneNumberVerificationSampleFactory : ISwaggerSampleFactory<PhoneNumberVerificationDTO>
    {
        public PhoneNumberVerificationDTO GetSampleData()
        {
            return new PhoneNumberVerificationDTO
            {
                Error = string.Empty,
                PhoneNumber = "+1-234-567-890",
                ClientName = "Your Name",
                Message = "Hey here is your code",
                ClientId = "8FAB9D02-B731-424B-AE06-56B2C90BAA0A",
                TerminalId = 1,
                TerminalName = "terminalFr8Core"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}