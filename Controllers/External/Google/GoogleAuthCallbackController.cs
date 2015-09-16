using System.Web.Mvc;
using Core.Managers;
using Core.Managers.APIManagers.Authorizers.Google;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Mvc.Controllers;

namespace Web.Controllers.External.Google
{
    [DockyardAuthorize]
    public class GoogleAuthCallbackController : AuthCallbackController
    {
        protected override FlowMetadata FlowData
        {
            get { return new GoogleAuthorizer().CreateFlowMetadata(this.GetUserId()); }
        }

        protected override ActionResult OnTokenError(global::Google.Apis.Auth.OAuth2.Responses.TokenErrorResponse errorResponse)
        {
            return RedirectToAction("MyAccount", "User");
        }
    }
}