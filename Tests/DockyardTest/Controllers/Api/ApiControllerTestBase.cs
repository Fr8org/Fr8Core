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
    /// <summary>
    /// A base class for Api controllers unit testing
    /// </summary>
    public abstract class ApiControllerTestBase : BaseTest
    {
        /// <summary>
        /// Creates an API controller with optional authorization context
        /// </summary>
        /// <typeparam name="TController">API controller type</typeparam>
        /// <param name="userId">User ID. Null or empty if no authorization context needed.</param>
        /// <param name="userRoles">User roles</param>
        /// <returns></returns>
        protected static TController CreateController<TController>(string userId = null, string[] userRoles = null)
            where TController : ApiController, new()
        {
            var controller = new TController();
            if (!string.IsNullOrEmpty(userId))
            {
                controller.User = new GenericPrincipal(new GenericIdentity(userId, "Forms"), userRoles);
            }
            return controller;
        }
    }
}
