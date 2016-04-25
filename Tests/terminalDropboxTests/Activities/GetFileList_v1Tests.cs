using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Interfaces;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;
using terminalDropbox.Actions;
using terminalDropboxTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;

namespace terminalDropboxTests.Activities
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
        public void Run_ReturnsPayloadDTO()
        {
            //Arrange
            var curActivityDO = FixtureData.GetFileListTestActivityDO1();
            var container = FixtureData.TestContainer();
            //Act
            var payloadDTOResult = _getFileList_v1.Run(curActivityDO, container.Id, FixtureData.DropboxAuthorizationToken()).Result;
            var jsonData = ((JValue)(payloadDTOResult.CrateStorage.Crates[1].Contents)).Value.ToString();
            var dropboxFileList = JsonConvert.DeserializeObject<List<string>>(jsonData);
            
            // Assert
            Assert.NotNull(payloadDTOResult);
            Assert.True(dropboxFileList.Any());
        }
    }
}
