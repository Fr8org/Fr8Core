using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IHMACAuthenticator
    {
        Task<bool> IsValidRequest(HttpRequestMessage request, string terminalSecret);
        void ExtractTokenParts(HttpRequestMessage request, out string terminalId, out string userId);
    }
}
