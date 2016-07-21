using System.Net.Http;
using System.Threading.Tasks;

namespace Fr8Infrastructure.Interfaces
{
    public interface IHMACAuthenticator
    {
        Task<bool> IsValidRequest(HttpRequestMessage request, string terminalSecret);
        void ExtractTokenParts(HttpRequestMessage request, out string terminalId, out string userId);
    }
}
