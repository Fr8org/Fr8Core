using System;

namespace Fr8.Infrastructure.Documentation.Swagger
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
                TerminalId = Guid.Parse("2757F870-A508-429E-A706-9EE826D92237"),
                TerminalName = "terminalFr8Core"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}