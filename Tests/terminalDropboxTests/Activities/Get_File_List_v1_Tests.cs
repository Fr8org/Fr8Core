using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Models;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDropbox.Actions;
using terminalDropboxTests.Fixtures;
using Fr8.Testing.Unit;
using terminalDropbox.Interfaces;

namespace terminalDropboxTests.Activities
{
    [TestFixture]
    [Category("terminalDropboxTests")]
    public class Get_File_List_v1_Tests : BaseTest
    {
        private Get_File_List_v1 _getFileList_v1;

        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();

            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.FromResult(FixtureData.FakePayloadDTO));

            var dropboxServiceMock = new Mock<IDropboxService>();

            dropboxServiceMock.Setup(x => x.GetFileList(It.IsAny<AuthorizationToken>())).Returns((AuthorizationToken x) => Task.FromResult(new List<string>()));

            ObjectFactory.Configure(cfg =>
            {
                cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object);
                cfg.For<IDropboxService>().Use(dropboxServiceMock.Object);
            });
            

            _getFileList_v1 = ObjectFactory.GetInstance<Get_File_List_v1>();
            //_getFileList_v1.HubCommunicator.Configure("terminalDropbox");
        }

        [Test]
        public async Task Initialize_ReturnsActivityDto()
        {
            //Arrange
            var curActivityContext = FixtureData.GetFileListActivityDO();
            AuthorizationToken tokenDTO = FixtureData.DropboxAuthorizationToken();
            curActivityContext.AuthorizationToken = tokenDTO;
            //Act
            await _getFileList_v1.Configure(curActivityContext);

            // Assert
            Assert.True(curActivityContext.ActivityPayload.CrateStorage.Count > 0);
        }
    }
}