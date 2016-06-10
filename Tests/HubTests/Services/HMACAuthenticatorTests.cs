using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Security;
using Moq;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;

namespace HubTests.Services
{
    [TestFixture]
    [Category("HMACAuthenticator")]
    public class HMACAuthenticatorTests : BaseTest
    {
        private IHMACAuthenticator _hmacAuthenticator;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var fr8HMACService = new Mock<IHMACService>();
            fr8HMACService.Setup(x => x.CalculateHMACHash(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync("authToken");
            ObjectFactory.Configure(o => o.For<IHMACService>().Use(fr8HMACService.Object));
            _hmacAuthenticator = new HMACAuthenticator(fr8HMACService.Object);
        }

        private HttpRequestMessage GetInCorrectRequestMessage1()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            HttpRequestHeaders headers = request.Headers;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("scheme");
            headers.Authorization = authorization;
            return request;
        }

        private HttpRequestMessage GetInCorrectRequestMessage2()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            HttpRequestHeaders headers = request.Headers;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("hmac", "terminalId:authToken:nonce:requestTime:userId");
            headers.Authorization = authorization;
            return request;
        }

        private HttpRequestMessage GetInCorrectRequestMessage3()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            HttpRequestHeaders headers = request.Headers;
            var requestTime = GetCurrentUnixTimestampSeconds() - 70;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("hmac", "terminalId:authToken:nonce:" + requestTime + ":userId");
            headers.Authorization = authorization;
            return request;
        }

        private HttpRequestMessage GetInCorrectRequestMessage4()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            HttpRequestHeaders headers = request.Headers;
            var requestTime = GetCurrentUnixTimestampSeconds() - 20;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("hmac", "terminalId:incorrect:nonce:" + requestTime + ":userId");
            headers.Authorization = authorization;
            return request;
        }

        private static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        private HttpRequestMessage GetCorrectRequestMessage()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            HttpRequestHeaders headers = request.Headers;
            var requestTime = GetCurrentUnixTimestampSeconds() - 20;
            AuthenticationHeaderValue authorization = new AuthenticationHeaderValue("hmac", "terminalId:authToken:nonce:"+ requestTime+ ":userId");
            headers.Authorization = authorization;
            return request;
        }

        [Test]
        public void HMACAuthenticatorService_ExtractTokenParts_ShouldReturnNullWithEmptyHMACHeader()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            string terminalId, userId;
            var requestMessage = GetInCorrectRequestMessage1();
            _hmacAuthenticator.ExtractTokenParts(requestMessage, out terminalId, out userId);

            Assert.AreEqual(null, terminalId);
            Assert.AreEqual(null, userId);
        }

        [Test]
        public void HMACAuthenticatorService_ExtractTokenParts_ShouldExtractHMACHeader()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            string terminalId, userId;
            var requestMessage = GetCorrectRequestMessage();
            _hmacAuthenticator.ExtractTokenParts(requestMessage, out terminalId, out userId);

            Assert.AreEqual("terminalId", terminalId);
            Assert.AreEqual("userId", userId);
        }

        [Test]
        public async Task HMACAuthenticatorService_IsValidRequest_ShouldReturnFalse_OnInvalidToken()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            var requestMessage = GetInCorrectRequestMessage1();
            var result = await _hmacAuthenticator.IsValidRequest(requestMessage, "testSecret");
            Assert.IsFalse(result);
        }

        [Test]
        public async Task HMACAuthenticatorService_IsValidRequest_ShouldReturnTrue_ValidRequest()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            var requestMessage = GetCorrectRequestMessage();
            var result = await _hmacAuthenticator.IsValidRequest(requestMessage, "testSecret");
            Assert.IsTrue(result);
        }

        [Test]
        public async Task HMACAuthenticatorService_IsValidRequest_ShouldReturnFalse_OnIncorrectTime()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            var requestMessage = GetInCorrectRequestMessage2();
            var result = await _hmacAuthenticator.IsValidRequest(requestMessage, "testSecret");
            Assert.IsFalse(result);
        }

        [Test]
        public async Task HMACAuthenticatorService_IsValidRequest_ShouldReturnFalse_OnOldTime()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            var requestMessage = GetInCorrectRequestMessage3();
            var result = await _hmacAuthenticator.IsValidRequest(requestMessage, "testSecret");
            Assert.IsFalse(result);
        }

        [Test]
        public async Task HMACAuthenticatorService_IsValidRequest_ShouldReturnFalse_OnNullTerminalSecret()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            var requestMessage = GetCorrectRequestMessage();
            var result = await _hmacAuthenticator.IsValidRequest(requestMessage, null);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task HMACAuthenticatorService_IsValidRequest_ShouldReturnFalse_OnInvalidAuthToken()
        {
            //lets remove nonce which was used on requests before us
            MemoryCache.Default.Remove("nonce");
            var requestMessage = GetInCorrectRequestMessage4();
            var result = await _hmacAuthenticator.IsValidRequest(requestMessage, "testSecret");
            Assert.IsFalse(result);
        }



    }
}
