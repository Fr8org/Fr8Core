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
    public class Query_DocuSign_v1_Tests : BaseHealthMonitorTest
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
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async void Query_DocuSign_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();

            requestActionDTO.AuthToken = null;

            await HttpPostAsync<ActionDTO, ActionDTO>(
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
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No AuthToken provided.""}"
        )]
        public async void Query_DocuSign_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;
            requestActionDTO.CrateStorage = Crate.ToDto(CreateConfiguredStorage());

            await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, requestActionDTO);
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""Action was not configured correctly""}"
        )]
        public async void Query_DocuSign_Run_NoConfig()
        {
            var runUrl = GetTerminalRunUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = HealthMonitor_FixtureData.DocuSign_AuthToken();

            await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, requestActionDTO);
        }
    }
}
