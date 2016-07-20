using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace HubWeb.ExceptionHandling
{
    public partial class Fr8ExceptionHandler
    {
        private class ErrorResult : IHttpActionResult
        {
            public HttpRequestMessage Request
            {
                get;
                set;
            }

            public ErrorDTO Content
            {
                get;
                set;
            }
            
            public ErrorResult(HttpRequestMessage request, ErrorDTO content)
            {
                Request = request;
                Content = content;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

                response.Content = new StringContent(JsonConvert.SerializeObject(Content), Encoding.UTF8, "application/json");
                response.RequestMessage = Request;
                
                return Task.FromResult(response);
            }
        }
    }
}