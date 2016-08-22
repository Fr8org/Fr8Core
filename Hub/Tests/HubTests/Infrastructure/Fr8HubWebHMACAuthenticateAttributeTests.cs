using NUnit.Framework;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Data.Entities;
using Fr8.Infrastructure.Interfaces;
using Hub.Infrastructure;
using Hub.Interfaces;
using Moq;
using StructureMap;
using Fr8.Testing.Unit;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Infrastructure
{
    [TestFixture]
    [Category("Fr8TerminalAuthenticationAttribute")]
    public class Fr8TerminalAuthenticationAttributeTests : BaseTest
    {
        private HttpAuthenticationContext _authenticationContext;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            HttpRequestMessage request = new HttpRequestMessage();
            HttpControllerContext controllerContext = new HttpControllerContext { Request = request };
            HttpActionContext context = new HttpActionContext { ControllerContext = controllerContext };
            _authenticationContext = new HttpAuthenticationContext(context, null);

            HttpRequestHeaders headers = request.Headers;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("FR8-TOKEN", "key=test, user=test");
            headers.Authorization = authorization;
        }

        private Fr8TerminalAuthenticationAttribute CreateFilter()
        {
            return new Fr8TerminalAuthenticationAttribute();
        }

        /// <summary>
        /// Mocked HMACAuthenticator returns true by default
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ShouldSetCurrentUser_WithCorrectAuthentication()
        {
            var terminalService = new Mock<ITerminal>();
            terminalService.Setup(x => x.GetByToken(It.IsAny<string>())).ReturnsAsync(new TerminalDO());
            ObjectFactory.Configure(o => o.For<ITerminal>().Use(terminalService.Object));

            await CreateFilter().AuthenticateAsync(_authenticationContext, CancellationToken.None);
            Assert.AreEqual(true, _authenticationContext.Principal is Fr8Principal);
            Assert.AreEqual(true, _authenticationContext.Principal.Identity is Fr8Identity);
        }

        [Test]
        public async Task ShouldntSetCurrentUser_WithInCorrectAuthentication()
        {
            HttpRequestHeaders headers = _authenticationContext.Request.Headers;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("FR8-TOKEN", "sdasdasd");
            headers.Authorization = authorization;

            await CreateFilter().AuthenticateAsync(_authenticationContext, CancellationToken.None);
            Assert.AreEqual(null, _authenticationContext.Principal);
        }

        [Test]
        public async Task ShouldntSetCurrentUser_WithInvalidTerminalToken()
        {
            var terminalService = new Mock<ITerminal>();
            terminalService.Setup(x => x.GetByToken(It.IsAny<string>())).ReturnsAsync(null);
            ObjectFactory.Configure(o => o.For<ITerminal>().Use(terminalService.Object));
            await CreateFilter().AuthenticateAsync(_authenticationContext, CancellationToken.None);
            Assert.AreEqual(null, _authenticationContext.Principal);
        }



    }
}