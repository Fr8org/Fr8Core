using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Interfaces;
using terminalAsanaTests.Fixtures;

namespace terminalAsanaTests.Unit
{
    class BaseAsanaTest
    {
        protected IRestfulServiceClient _restClient;
        protected IAsanaParameters _parameters;

        protected Mock<IRestfulServiceClient> _restClientMock;
        protected Mock<IAsanaParameters> _parametersMock;

        [TestFixtureSetUp]
        public void Startup()
        {
            var tokenUrl = "http://token.asana.com/";
            var anyUrl = "http://asana.com";


            _restClientMock = new Mock<IRestfulServiceClient>();
            _restClient = _restClientMock.Object;

            _parametersMock = new Mock<IAsanaParameters>();
            _parametersMock.Setup(x => x.MinutesBeforeTokenRenewal).Returns("10");

            // Url`s setup
            _parametersMock.Setup(x => x.AsanaOAuthTokenUrl).Returns(tokenUrl);
            _parametersMock.Setup(x => x.WorkspacesUrl).Returns(anyUrl);
            _parametersMock.Setup(x => x.UsersInWorkspaceUrl).Returns(anyUrl+"/workspaces");
            _parametersMock.Setup(x => x.UsersMeUrl).Returns(anyUrl+"/me");
            _parametersMock.Setup(x => x.UsersUrl).Returns(anyUrl+"/users");
            _parametersMock.Setup(x => x.TasksUrl).Returns(anyUrl+"/tasks");
            _parametersMock.Setup(x => x.StoriesUrl).Returns(anyUrl+"/stories");

            _parameters = _parametersMock.Object;


            //for token refreshing
            _restClientMock.Setup(
               x => x.PostAsync<JObject>(It.Is<Uri>( y => y.AbsoluteUri.Equals(tokenUrl)),
                                           It.IsAny<HttpContent>(),
                                           It.IsAny<string>(),
                                           It.IsAny<Dictionary<string, string>>()))

                      .ReturnsAsync(JObject.Parse(FixtureData.SampleAsanaOauthTokenResponse()));
        }

    }
}
