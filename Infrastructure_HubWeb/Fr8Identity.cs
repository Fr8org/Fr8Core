// TODO: FR-3320, moved to Hub.Infrastructure.
// using Microsoft.AspNet.Identity;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Security.Principal;
// using System.Web;
// 
// namespace HubWeb.Infrastructure
// {
//     public sealed class Fr8Identity : ClaimsIdentity
//     {
//         public Fr8Identity(string name, string userId) : base ("hmac")
//         {
//             AddClaim(new Claim(ClaimTypes.Name, name));
//             AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
//         }
//     }
// }