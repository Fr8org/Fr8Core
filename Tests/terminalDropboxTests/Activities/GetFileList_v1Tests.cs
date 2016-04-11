using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Hub.Interfaces;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;
using terminalDropbox.Actions;
using terminalDropbox.Interfaces;
using terminalDropbox.Services;
using terminalDropboxTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;

namespace terminalDropboxTests.Actions
{
    [TestFixture]
    [Category("terminalDropboxTests")]
    public class GetFileList_v1Tests : BaseTest
    {
        private Get_File_List_v1 _getFileList_v1;

        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();

            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.FromResult(FixtureData.FakePayloadDTO));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));
            
            _getFileList_v1 = ObjectFactory.GetInstance<Get_File_List_v1>();
            _getFileList_v1.HubCommunicator.Configure("terminalDropbox");
        }

        [Test]
        public void Initialize_ReturnsActivityDto()
        {
            //Arrange
            var curActivityDO = FixtureData.GetFileListTestActivityDO1();


            //Act
            var activityDto = _getFileList_v1.Configure(curActivityDO, FixtureData.DropboxAuthorizationToken()).Result;
            
            // Assert
            Assert.NotNull(activityDto);
        }
    }
}