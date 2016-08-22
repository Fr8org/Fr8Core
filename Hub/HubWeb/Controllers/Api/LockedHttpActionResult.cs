using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace HubWeb.Controllers.Api
{
    public class LockedHttpActionResult : IHttpActionResult
    {
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.StatusCode = (HttpStatusCode)423;

            return Task.FromResult(response);
        }
    }
}