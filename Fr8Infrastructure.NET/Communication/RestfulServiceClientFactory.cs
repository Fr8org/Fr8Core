using Fr8.Infrastructure.Interfaces;

namespace Fr8.Infrastructure.Communication
{
    public class RestfulServiceClientFactory : IRestfulServiceClientFactory
    {
        public IRestfulServiceClient Create()
        {
            return new RestfulServiceClient();
        }
    }
}
