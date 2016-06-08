using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Interfaces;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDropbox.Actions;
using terminalDropboxTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using terminalDropbox.Interfaces;
using TerminalBase.Models;

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
            ObjectFactory.Configure(cfg =>
            {
                cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object);
                cfg.For<IDropboxService>().Use(Mock.Of<IDropboxService>());
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