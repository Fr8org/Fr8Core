using System;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDropbox.Actions;
using terminalDropboxTests.Fixtures;
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
            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>()))
                .Returns(Task.FromResult(FixtureData.FakePayloadDTO));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));

            _getFileList_v1 = ObjectFactory.GetInstance<Get_File_List_v1>();
        }

        [Test]
        public void Run_ReturnsPayloadDTO()
        {
            //Arrange
            var curActionDO = FixtureData.GetFileListTestActionDO1();
            var container = FixtureData.TestContainer();
            
            //Act
            var payloadDTOResult  =  _getFileList_v1.Run(curActionDO, container.Id, FixtureData.DropboxAuthorizationToken()).Result;
            
            // Assert
            Assert.NotNull(payloadDTOResult);
        }
    }
}
