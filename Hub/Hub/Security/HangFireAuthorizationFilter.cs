using Hangfire.Dashboard;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Security
{
    public class HangFireAuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            var context = new OwinContext(owinEnvironment);
            if (context.Authentication.User.IsInRole("Admin"))
                return true;
            else return false;
        }
    }
}
