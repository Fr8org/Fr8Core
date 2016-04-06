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
            if (actionContext.RequestContext.Principal.IsInRole("Guest"))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden,
                    ErrorDTO.AuthenticationError("You need to register before using this functionality.", null, "GuestFail"));
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden,
                    ErrorDTO.AuthenticationError("Authorization has been denied for this request."));
            }
        }
    }
}