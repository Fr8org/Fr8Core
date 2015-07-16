using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Core.Managers;
using Core.Managers.APIManagers.Authorizers;
using Core.Managers.APIManagers.Authorizers.Docusign;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers.External.DocuSign
{
    [KwasantAuthorize]
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
                var authorizer = (DocusignAuthorizer)ObjectFactory.GetNamedInstance<IOAuthAuthorizer>("Docusign");
                try
                {
                    await authorizer.ObtainAccessTokenAsync(this.GetUserId(), loginVm.Username, loginVm.Password);
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(ViewBag.ReturnUrl);
                    else
                        return RedirectToAction("MyAccount", "User");
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