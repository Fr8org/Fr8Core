using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class CredentialsSampleFactory : ISwaggerSampleFactory<CredentialsDTO>
    {
        private readonly ISwaggerSampleFactory<TerminalSummaryDTO> _terminalSampleFactory;
        public CredentialsSampleFactory(ISwaggerSampleFactory<TerminalSummaryDTO> terminalSampleFactory)
        {
            _terminalSampleFactory = terminalSampleFactory;
        }

        public CredentialsDTO GetSampleData()
        {
            return new CredentialsDTO
            {
                Terminal = _terminalSampleFactory.GetSampleData(),
                Domain = "http://yourdomain.com",
                Fr8UserId = "D7F645DF-53F1-40FF-BC73-8347AE57CE08",
                IsDemoAccount = false,
                Password = "your_secret_password",
                Username = "your_username"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}