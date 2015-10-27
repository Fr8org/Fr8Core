using System.Web.Mvc;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Mvc.Controllers;
using Hub.Managers;
using Hub.Managers.APIManagers.Authorizers.Google;

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