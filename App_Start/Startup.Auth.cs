// TODO: FR-3320, moved to Hub.
// using System;
// using Microsoft.AspNet.Identity;
// using Microsoft.Owin;
// using Microsoft.Owin.Security.Cookies;
// using Owin;
// using Microsoft.Owin.Security.DataProtection;
// using Hub.Security;
// 
// namespace HubWeb
// {
//     public partial class Startup
//     {
//         // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
//         public void ConfigureAuth(IAppBuilder app)
//         {
// 
//             // Enable the application to use a cookie to store information for the signed in user
//             app.UseCookieAuthentication(new CookieAuthenticationOptions
//             {
//                 AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
//                 ExpireTimeSpan = TimeSpan.FromHours(1),
//                 LoginPath = new PathString("/DockyardAccount/Index"),
//                 Provider = new CookieAuthenticationProvider
//                 {
//                     OnApplyRedirect =
//                         ctx =>
//                         {
//                             if (!IsJsonRequest(ctx.Request))
//                             {
//                                 ctx.Response.Redirect(ctx.RedirectUri);
//                             }
//                         }
//                 }});
// 
//             // Use a cookie to temporarily store information about a user logging in with a third party login provider
//             app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
// 
//             // Uncomment the following lines to enable logging in with third party login providers
//             //app.UseMicrosoftAccountAuthentication(
//             //    clientId: "",
//             //    clientSecret: "");
// 
//             //app.UseTwitterAuthentication(
//             //   consumerKey: "",
//             //   consumerSecret: "");
// 
//             //app.UseFacebookAuthentication(
//             //   appId: "",
//             //   appSecret: "");
// 
//             //app.UseGoogleAuthentication();
// 
//             DockyardIdentityManager.DataProtectionProvider = app.GetDataProtectionProvider();
// 
//         }
// 
//         private static bool IsJsonRequest(IOwinRequest request)
//         {
//             IReadableStringCollection query = request.Query;
//             if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
//             {
//                 return true;
//             }
//             IHeaderDictionary headers = request.Headers;
//             return ((headers != null) && ((headers["X-Requested-With"] == "XMLHttpRequest") || (headers["Content-Type"] == "application/json")));
//         }
//     }
// }