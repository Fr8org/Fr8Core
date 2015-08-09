using System.Web.Mvc;
using Google.Apis.Auth.OAuth2.Mvc;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Authorizers.Google;

namespace KwasantWeb.Controllers.External.GoogleCalendar
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