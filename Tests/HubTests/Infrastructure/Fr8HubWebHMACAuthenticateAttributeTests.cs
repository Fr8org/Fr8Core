using NUnit.Framework;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using fr8.Infrastructure.Interfaces;
using Hub.Infrastructure;
using Hub.Interfaces;
using Moq;
using StructureMap;
using UtilitiesTesting;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Infrastructure
{
    [TestFixture]
    [Category("Fr8HubWebHMACAuthenticateAttribute")]
    public class Fr8HubWebHMACAuthenticateAttributeTests : BaseTest
    {
        private HttpAuthenticationContext _authenticationContext;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            HttpRequestMessage request = new HttpRequestMessage();
            HttpControllerContext controllerContext = new HttpControllerContext {Request = request};
            HttpActionContext context = new HttpActionContext {ControllerContext = controllerContext};
            _authenticationContext = new HttpAuthenticationContext(context, null);
            HttpRequestHeaders headers = request.Headers;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("scheme");
            headers.Authorization = authorization;
        }

        private Fr8HubWebHMACAuthenticateAttribute CreateFilter()
        {
            return new Fr8HubWebHMACAuthenticateAttribute();
        }

        /// <summary>
        /// Mocked HMACAuthenticator returns true by default
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ShouldSetCurrentUser_WithCorrectAuthentication()
        {
            await CreateFilter().AuthenticateAsync(_authenticationContext, CancellationToken.None);
            Assert.AreEqual(true, _authenticationContext.Principal is Fr8Principle);
            Assert.AreEqual(true, _authenticationContext.Principal.Identity is Fr8Identity);
        }

        [Test]
        public async Task ShouldntSetCurrentUser_WithInCorrectAuthentication()
        {
            var fr8HMACAuthenticator = new Mock<IHMACAuthenticator>();
            fr8HMACAuthenticator.Setup(x => x.IsValidRequest(It.IsAny<HttpRequestMessage>(), It.IsAny<string>())).ReturnsAsync(false);
            var outTerminalId = "testTerminal";
            var outUserId = "testUser";
            fr8HMACAuthenticator.Setup(s => s.ExtractTokenParts(It.IsAny<HttpRequestMessage>(), out outTerminalId, out outUserId));
            ObjectFactory.Configure(o => o.For<IHMACAuthenticator>().Use(fr8HMACAuthenticator.Object));

            await CreateFilter().AuthenticateAsync(_authenticationContext, CancellationToken.None);
            Assert.AreEqual(null, _authenticationContext.Principal);
        }

        [Test]
        public async Task ShouldntSetCurrentUser_WithNullUserId()
        {
            var fr8HMACAuthenticator = new Mock<IHMACAuthenticator>();
            fr8HMACAuthenticator.Setup(x => x.IsValidRequest(It.IsAny<HttpRequestMessage>(), It.IsAny<string>())).ReturnsAsync(true);
            var outTerminalId = "testTerminal";
            string outUserId = null;
            fr8HMACAuthenticator.Setup(s => s.ExtractTokenParts(It.IsAny<HttpRequestMessage>(), out outTerminalId, out outUserId));
            ObjectFactory.Configure(o => o.For<IHMACAuthenticator>().Use(fr8HMACAuthenticator.Object));

            await CreateFilter().AuthenticateAsync(_authenticationContext, CancellationToken.None);
            Assert.AreEqual(null, _authenticationContext.Principal);
        }

        [Test]
        public async Task ShouldntSetCurrentUser_WithInvalidTerminalId()
        {
            var terminalService = new Mock<ITerminal>();
            terminalService.Setup(x => x.GetTerminalByPublicIdentifier(It.IsAny<string>())).ReturnsAsync(null);
            ObjectFactory.Configure(o => o.For<ITerminal>().Use(terminalService.Object));

            await CreateFilter().AuthenticateAsync(_authenticationContext, CancellationToken.None);
            Assert.AreEqual(null, _authenticationContext.Principal);
        }



    }
}