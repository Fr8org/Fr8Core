using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using Hub.Managers.APIManagers.Transmitters.Restful;
using NUnit.Framework;
using terminalDocuSign.Actions;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Query_DocuSign_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private CrateStorage CreateConfiguredStorage()
        {
            var storage = new CrateStorage();

            storage.Add(Data.Crates.Crate.FromContent("Config", new Query_DocuSign_v1.ActionUi()));
            
            return storage;
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}",
            MatchType = MessageMatch.Contains
        )]
        public async void Query_DocuSign_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();

            requestActionDTO.AuthToken = null;

            await HttpPostAsync<ActivityDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
               );
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}",
            MatchType = MessageMatch.Contains
        )]
        public async void Query_DocuSign_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;
            requestActionDTO.CrateStorage = Crate.ToDto(CreateConfiguredStorage());

            await HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, requestActionDTO);
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""Action was not configured correctly""}",
            MatchType = MessageMatch.Contains
        )]
        public async void Query_DocuSign_Run_NoConfig()
        {
            var runUrl = GetTerminalRunUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = HealthMonitor_FixtureData.DocuSign_AuthToken();

            await HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, requestActionDTO);
        }

        [Test]
        public async void Query_DocuSign_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async void Query_DocuSign_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
