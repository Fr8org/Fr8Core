using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using HubWeb;
using HubWeb.Controllers;
using HubWeb.Infrastructure;
using NUnit.Framework;
using UtilitiesTesting;

namespace HubTests.Controllers.Api
{
    /// <summary>
    /// A base class for Api controllers unit testing
    /// </summary>
    public abstract class ApiControllerTestBase : BaseTest
    {
        protected void ShouldHaveFr8ApiAuthorize(Type controllerType)
        {
            var authAttribute = controllerType.GetCustomAttributes(typeof(Fr8ApiAuthorizeAttribute), true);
            Assert.IsTrue(authAttribute.Any());
        }

        protected void ShouldHaveFr8ApiAuthorizeOnFunction(Type controllerType, string functionName)
        {
            var methodInfo = controllerType.GetMethod(functionName);
            var authAttribute = methodInfo.GetCustomAttributes(typeof(Fr8ApiAuthorizeAttribute), true);
            Assert.IsTrue(authAttribute.Any());
        }

        protected void ShouldHaveFr8HMACAuthorize(Type controllerType)
        {
            var authAttribute = controllerType.GetCustomAttributes(typeof(Fr8HubWebHMACAuthenticateAttribute), true);
            Assert.IsTrue(authAttribute.Any());
        }

        protected void ShouldHaveFr8HMACAuthorizeOnFunction(Type controllerType, string functionName)
        {
            var methodInfo = controllerType.GetMethod(functionName);
            ShouldHaveFr8HMACAuthorizeOnFunction(methodInfo);
        }

        protected void ShouldHaveFr8HMACAuthorizeOnFunction(MethodInfo method)
        {
            var authAttribute = method.GetCustomAttributes(typeof(Fr8HubWebHMACAuthenticateAttribute), true);
            Assert.IsTrue(authAttribute.Any());
        }
    }
}
