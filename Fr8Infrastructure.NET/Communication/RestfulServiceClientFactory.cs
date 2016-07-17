using Fr8.Infrastructure.Interfaces;

namespace Fr8.Infrastructure.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public class RestfulServiceClientFactory : IRestfulServiceClientFactory
    {
        public IRestfulServiceClient Create(IRequestSignature signature)
        {
            var rsc = new RestfulServiceClient();
            if(signature != null)
            { 
                rsc.AddRequestSignature(signature);
            }
            return rsc;
        }
    }
}
