using System.Net.Http;

namespace Fr8.Infrastructure.Interfaces
{
    public interface IRequestSignature
    {
        void SignRequest(HttpRequestMessage request);
    }
}