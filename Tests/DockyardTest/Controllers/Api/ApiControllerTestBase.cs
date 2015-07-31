using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NUnit.Framework;
using UtilitiesTesting;

namespace DockyardTest.Controllers.Api
{
    [TestFixture]
    public abstract class ApiControllerTestBase : BaseTest
    {
        protected static TController CreateController<TController>(string userId = null, string[] userRoles = null)
            where TController : ApiController, new()
        {
            var ptc = new TController();
            if (!string.IsNullOrEmpty(userId))
            {
                ptc.User = new GenericPrincipal(new GenericIdentity(userId, "Forms"), userRoles);
            }
            return ptc;
        }
    }
}
