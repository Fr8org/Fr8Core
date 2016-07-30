using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Hub.Infrastructure;
using HubWeb.Infrastructure;
using Fr8.Testing.Unit;
using HubWeb.Infrastructure_HubWeb;

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

        protected void ShouldHaveFr8ApiAuthorizeOnFunction(Type controllerType, string functionName, Type[] types = null)
        {
            MethodInfo methodInfo;
            if (types == null)
            {
                methodInfo = controllerType.GetMethod(functionName);
            }
            else
            {
                methodInfo = controllerType.GetMethod(functionName, types);
            }
            var authAttribute = methodInfo.GetCustomAttributes(typeof(Fr8ApiAuthorizeAttribute), true);
            Assert.IsTrue(authAttribute.Any());
        }

        protected void ShouldHaveFr8HMACAuthorize(Type controllerType)
        {
            var authAttribute = controllerType.GetCustomAttributes(typeof(Fr8TerminalAuthenticationAttribute), true);
            Assert.IsTrue(authAttribute.Any());
        }

        protected void ShouldHaveFr8HMACAuthorizeOnFunction(Type controllerType, string functionName)
        {
            var methodInfo = controllerType.GetMethod(functionName);
            ShouldHaveFr8HMACAuthorizeOnFunction(methodInfo);
        }

        protected void ShouldHaveFr8HMACAuthorizeOnFunction(MethodInfo method)
        {
            var authAttribute = method.GetCustomAttributes(typeof(Fr8TerminalAuthenticationAttribute), true);
            Assert.IsTrue(authAttribute.Any());
        }
    }
}
