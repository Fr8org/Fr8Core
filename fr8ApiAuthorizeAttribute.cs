using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Data.Interfaces.DataTransferObjects;

namespace Web
{
    public class fr8ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public fr8ApiAuthorizeAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var response = actionContext.Request.CreateResponse(HttpStatusCode.InternalServerError, ErrorDTO.AuthenticationError("Authorization has been denied for this request."));
            actionContext.Response = response;
        }
    }
}