using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Data.Interfaces.DataTransferObjects;

namespace HubWeb
{
    public class Fr8ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public Fr8ApiAuthorizeAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, ErrorDTO.AuthenticationError("Authorization has been denied for this request."));
            actionContext.Response = response;
        }
    }
}