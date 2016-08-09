using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PhoneNumberCredentialsSampleFactory : ISwaggerSampleFactory<PhoneNumberCredentialsDTO>
    {
        private readonly ISwaggerSampleFactory<TerminalDTO> _terminalSampleFactory;
        public PhoneNumberCredentialsSampleFactory(ISwaggerSampleFactory<TerminalDTO> terminalSampleFactory)
        {
            _terminalSampleFactory = terminalSampleFactory;
        }

        public PhoneNumberCredentialsDTO GetSampleData()
        {
            return new PhoneNumberCredentialsDTO
            {
                Terminal = _terminalSampleFactory.GetSampleData(),
                Fr8UserId = "F74FC833-FD7E-429B-8A97-B4BD31EDCCF3",
                Error = string.Empty,
                ClientId = "43308C96-57AA-463A-B45C-31F34C8748EF",
                ClientName = "Your Name",
                Message = "Hey there your code is here",
                PhoneNumber = "+1-233-567-890",
                VerificationCode = "1234"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}