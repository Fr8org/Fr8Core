using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using Hub.Managers;
using Hub.Managers.APIManagers.Authorizers;
using Hub.Managers.APIManagers.Authorizers.Docusign;
using HubWeb.ViewModels;

namespace HubWeb.Controllers.External.DocuSign
{
    [DockyardAuthorize]
    public class DocusignAuthCallbackController : Controller
    {
        // GET: DocusignAuthCallback/IndexAsync
        public ActionResult IndexAsync()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(DocusignLoginVM loginVm, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var authorizer = (IDocusignAuthorizer)ObjectFactory.GetNamedInstance<IOAuthAuthorizer>("Docusign");
                try
                {
                    await authorizer.ObtainAccessTokenAsync(this.GetUserId(), loginVm.Username, loginVm.Password);
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(ViewBag.ReturnUrl);
                    else
                        return RedirectToAction("RemoteServices", "User");
                }
                catch (OAuthException ex)
                {
                    ModelState.AddModelError("", ex);
                }
            }
            return View();
        }
    }
}