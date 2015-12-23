using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.StructureMap;
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
    public class Post_To_Yammer_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalYammer"; }
        }

        private async Task<ActionDTO> ConfigurationRequest()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Post_To_Yammer_v1_InitialConfiguration_ActionDTO();

            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            var storage = Crate.GetStorage(responseActionDTO);

            using (var updater = Crate.UpdateStorage(requestActionDTO))
            {
                updater.CrateStorage = storage;
            }

            return await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);
        }

        private async Task<ActionDTO> ConfigureInitial(bool isAuthToken = true)
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Post_To_Yammer_v1_InitialConfiguration_ActionDTO(isAuthToken);
            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            return responseActionDTO;
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Available Fields"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Available Groups"));
        }

        [Test]
        public async void Post_To_Yammer_v1_Initial_Configuration_Check_Crate_Structure()
        {
            // Act
            var responseActionDTO = await ConfigureInitial();

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
        }

        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async void Post_To_Yammer_v1_Initial_Configuration_Check_Crate_Structure_NoAuth()
        {
            // Act
            var responseActionDTO = await ConfigureInitial(false);
        }


        [Test]
        public async void Post_To_Yammer_v1_FollowupConfiguration()
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
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No selected group found in action.""}"
        )]
        public async void Post_To_Yammer_Run_Return_Payload()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            var actionDTO = await ConfigurationRequest();

            AddPayloadCrate(
                actionDTO,
                new StandardPayloadDataCM(
                    new FieldDTO("message", "Hello")
                ),
                "Payload crate"
            );

            actionDTO.AuthToken = HealthMonitor_FixtureData.Yammer_AuthToken();
            //Act
            var responsePayloadDTO =
                await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

            //Assert
            var crateStorage = Crate.FromDto(responsePayloadDTO.CrateStorage);

            var StandardPayloadDataCM = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().SingleOrDefault();

            Assert.IsNotNull(StandardPayloadDataCM);
        }
    }
}
