using System.Web.Mvc;
using Google.Apis.Auth.OAuth2.Mvc;
using Core.Managers;
using Core.Managers.APIManagers.Authorizers.Google;

namespace Web.Controllers.External.GoogleCalendar
{
    [KwasantAuthorize]
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected override FlowMetadata FlowData
        {
            get { return new GoogleCalendarAuthorizer().CreateFlowMetadata(this.GetUserId()); }
        }

        protected override ActionResult OnTokenError(Google.Apis.Auth.OAuth2.Responses.TokenErrorResponse errorResponse)
        {
            return RedirectToAction("MyAccount", "User");
        }
    }
}