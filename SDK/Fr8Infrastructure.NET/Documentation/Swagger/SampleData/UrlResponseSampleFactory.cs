using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class UrlResponseSampleFactory : ISwaggerSampleFactory<UrlResponseDTO>
    {
        public UrlResponseDTO GetSampleData()
        {
            return new UrlResponseDTO
            {
                Url = "https://slack.com/api/oauth.access?client_id=CLIENT_IDredirect_uri=REDIRECT_URL"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}