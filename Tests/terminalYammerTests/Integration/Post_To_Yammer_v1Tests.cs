using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalYammerTests.Fixtures;

namespace terminalYammerTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("Integration.terminalYammer")]
    public class Post_To_Yammer_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalYammer"; }
        }

        private async Task<ActivityDTO> ConfigurationRequest()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData.Post_To_Yammer_v1_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            var storage = Crate.GetStorage(responseActionDTO);

            using (var crateStorage = Crate.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }

            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
        }

        private async Task<ActivityDTO> ConfigureInitial(bool isAuthToken = true)
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Post_To_Yammer_v1_InitialConfiguration_Fr8DataDTO(isAuthToken);
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);

            return responseActionDTO;
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(2, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "Available Groups"));
        }

        [Test]
        public async Task Post_To_Yammer_v1_Initial_Configuration_Check_Crate_Structure()
        {
            // Act
            var responseActionDTO = await ConfigureInitial();

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
        }

        [Test]
        public async Task Post_To_Yammer_v1_Initial_Configuration_Check_Crate_Structure_NoAuth()
        {
            // Act
            var response = await ConfigureInitial(false);
            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
        }


        [Test]
        public async Task Post_To_Yammer_v1_FollowupConfiguration()
        {
            // Act
            var responseFollowUpActionDTO = await ConfigurationRequest();

            // Assert
            Assert.NotNull(responseFollowUpActionDTO);
        }

        // After Running the Post to yammer run method each time messagge will be posted on the group.
        // We haven't selected the group. We are expecting the exception from run method 
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException)
        )]
        public async Task Post_To_Yammer_Run_Return_Payload()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            var activityDTO = await ConfigurationRequest();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            AddPayloadCrate(
                dataDTO,
                new StandardPayloadDataCM(
                    new KeyValueDTO("message", "Hello")
                ),
                "Payload crate"
            );

            activityDTO.AuthToken = HealthMonitor_FixtureData.Yammer_AuthToken();
            
            //Act
            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            //Assert
            var crateStorage = Crate.FromDto(responsePayloadDTO.CrateStorage);
            var StandardPayloadDataCM = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().SingleOrDefault();

            Assert.IsNotNull(StandardPayloadDataCM);
           
        }
    }
}
