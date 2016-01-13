using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface IHMACService
    {
        Task<string> CalculateHMACHash<T>(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce, T content);
        Task<string> CalculateHMACHash(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce, HttpContent content);
        Task<string> CalculateHMACHash(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce);
    }
}
