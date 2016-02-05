using NUnit.Framework;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using UtilitiesTesting;

namespace HubWeb.Infrastructure
{
    [TestFixture]
    [Category("Fr8HubWebHMACAuthenticateAttribute")]
    public class Fr8HubWebHMACAuthenticateAttributeTests : BaseTest
    {
        private Fr8HubWebHMACAuthenticateAttribute _hmacAttribute;
        private HttpAuthenticationContext _authenticationContext;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _hmacAttribute = new Fr8HubWebHMACAuthenticateAttribute();
            HttpRequestMessage request = new HttpRequestMessage();
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = request;
            HttpActionContext context = new HttpActionContext();
            context.ControllerContext = controllerContext;
            _authenticationContext = new HttpAuthenticationContext(context, null);
            HttpRequestHeaders headers = request.Headers;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("scheme");
            headers.Authorization = authorization;
        }


        [Test]
        public async Task ShouldntSetCurrentUser_WithEmptyHMACHeader()
        {
            HttpContext.Current.User = null;
            var sampleTerminalId = Guid.NewGuid();
            await _hmacAttribute.AuthenticateAsync(_authenticationContext, CancellationToken.None);
            //check data
            int a = 12;
            Assert.AreEqual(null, HttpContext.Current.User);
        }



    }
}