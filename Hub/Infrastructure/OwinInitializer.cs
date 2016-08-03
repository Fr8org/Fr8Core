using System;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using Hub.Security;
using Microsoft.AspNet.Identity.Owin;
using StructureMap;

namespace Hub.Infrastructure
{
    public class OwinInitializer
    {
        public static void ConfigureAuth(IAppBuilder app, string loginPath)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // Enable the application to use a cookie to store information for the signed in user
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    ExpireTimeSpan = TimeSpan.FromHours(1),
                    LoginPath = new PathString(loginPath),
                    Provider = new CookieAuthenticationProvider
                    {
                        OnApplyRedirect =
                            ctx =>
                            {
                                if (!IsJsonRequest(ctx.Request))
                                {
                                    ctx.Response.Redirect(ctx.RedirectUri);
                                }
                            },
                        // This is a security feature which is used when you change a password or add an external login to your account.  
                        OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<DockyardIdentityManager, Fr8AccountDO>(
                        validateInterval: TimeSpan.FromSeconds(30),
                        regenerateIdentity: (manager, user) => ObjectFactory.GetInstance<ISecurityServices>().GetIdentityAsync(uow, user))
                    }
                });

                // Use a cookie to temporarily store information about a user logging in with a third party login provider
                app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

                DockyardIdentityManager.DataProtectionProvider = app.GetDataProtectionProvider();
            }
        }

        private static bool IsJsonRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && ((headers["X-Requested-With"] == "XMLHttpRequest") || (headers["Content-Type"] == "application/json")));
        }
    }
}
