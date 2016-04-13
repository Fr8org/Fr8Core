using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Hub.Managers;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Testing;
using Ploeh.AutoFixture;
using terminalDropbox;
using terminalDropboxTests.Fixtures;

namespace terminalDropboxTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Get_File_List_v1_Tests : BaseTerminalIntegrationTest
    {
        private const string Host = "http://localhost:19760";
        private IDisposable _app;
        private Fixture _fixture;

        public override string TerminalName => "terminalDropbox";

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _app = WebApp.Start<Startup>(Host);

            // AutoFixture Setup
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Register(() => new AuthorizationTokenDTO
            {
                Token = "bLgeJYcIkHAAAAAAAAAAFf6hjXX_RfwsFNTfu3z00zrH463seBYMNqBaFpbfBmqf"
            });
        }

        [TearDown]
        public void FixtureTearDown()
        {
            _app.Dispose();
        }

        [Test, Category("Integration.terminalDropbox")]
        public async Task GetFileList_InitialConfig_ReturnsActivity()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            ActivityTemplateDTO activityTemplateDto = _fixture.Build<ActivityTemplateDTO>()
                .With(x => x.Id)
                .With(x => x.Name, "Get_File_List_TEST")
                .With(x => x.Version, "1")
                .OmitAutoProperties()
                .Create();
            AuthorizationTokenDTO tokenDO = _fixture.Create<AuthorizationTokenDTO>();
            ActivityDTO activityDto = _fixture.Build<ActivityDTO>()
                .With(x => x.Id)
                .With(x => x.ActivityTemplate, activityTemplateDto)
                .With(x => x.CrateStorage, null)
                .With(x => x.AuthToken, tokenDO)
                .OmitAutoProperties()
                .Create();
            
            Fr8DataDTO requestActionDTO = new Fr8DataDTO() { ActivityDTO = activityDto };

            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                    );

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
        }

        [Test, Category("Integration.terminalDropbox")]
        public async Task Activate_Returns_ActivityDTO()
        {
            //Arrange
            var activateUrl = GetTerminalActivateUrl();

            ActivityTemplateDTO activityTemplateDto = _fixture.Build<ActivityTemplateDTO>()
               .With(x => x.Id)
               .With(x => x.Name, "Get_File_List_TEST")
               .With(x => x.Version, "1")
               .OmitAutoProperties()
               .Create();
            AuthorizationTokenDTO tokenDO = _fixture.Create<AuthorizationTokenDTO>();
            ActivityDTO activityDto = _fixture.Build<ActivityDTO>()
                .With(x => x.Id)
                .With(x => x.ActivityTemplate, activityTemplateDto)
                .With(x => x.CrateStorage, null)
                .With(x => x.AuthToken, tokenDO)
                .OmitAutoProperties()
                .Create();
            
            Fr8DataDTO dataDto = new Fr8DataDTO() { ActivityDTO = activityDto };
            
            // Add initial configuretion controls
            using (var crateStorage = Crate.GetUpdatableStorage(dataDto.ActivityDTO))
            {
                crateStorage.Add(Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", new ControlDefinitionDTO[] { }));
            }

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    activateUrl,
                    dataDto
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test, Category("Integration.terminalDropbox")]
        public async Task Run_Returns_ActivityDTO()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            ActivityTemplateDTO activityTemplateDto = _fixture.Build<ActivityTemplateDTO>()
               .With(x => x.Id)
               .With(x => x.Name, "Get_File_List_TEST")
               .With(x => x.Version, "1")
               .OmitAutoProperties()
               .Create();
            AuthorizationTokenDTO tokenDO = _fixture.Create<AuthorizationTokenDTO>();
            ActivityDTO activityDto = _fixture.Build<ActivityDTO>()
                .With(x => x.Id)
                .With(x => x.ActivityTemplate, activityTemplateDto)
                .With(x => x.CrateStorage, null)
                .With(x => x.AuthToken, tokenDO)
                .OmitAutoProperties()
                .Create();

            Fr8DataDTO dataDto = new Fr8DataDTO() { ActivityDTO = activityDto };

            // Add initial configuretion controls
            using (var crateStorage = Crate.GetUpdatableStorage(dataDto.ActivityDTO))
            {
                crateStorage.Add(Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", new ControlDefinitionDTO[] { }));
            }
            // Add operational state crate
            AddOperationalStateCrate(dataDto, new OperationalStateCM());

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    runUrl,
                    dataDto
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        //var dataDTO = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();
        //AddOperationalStateCrate(dataDTO, new OperationalStateCM());

        //var payloadDTOResult = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
        //var jsonData = ((JValue)(payloadDTOResult.CrateStorage.Crates[1].Contents)).Value.ToString();
        //var dropboxFileList = JsonConvert.DeserializeObject<List<string>>(jsonData);

        //Assert.NotNull(payloadDTOResult);
        //Assert.True(dropboxFileList.Any());
    }
}
