using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hub.Managers.APIManagers.Transmitters.Http
{
    /// <summary>
    /// Abstracts http channel with optional things such as additional authorization attempts and so on.
    /// </summary>
    public interface IAuthorizingHttpChannel
    {
        /// <summary>
        /// Http request may be sent several times in the scope of this method but some .NET http clients don't support sending the same request instance twice.
        /// </summary>
        /// <param name="requestFactoryMethod"></param>
        /// <param name="userId">DockYardAccount ID for authorization</param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactoryMethod, string userId);
    }
}