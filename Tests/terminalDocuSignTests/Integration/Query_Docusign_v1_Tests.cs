using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalDocuSignTests.Fixtures;
using terminalDocuSign.Activities;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Query_DocuSign_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private ICrateStorage CreateConfiguredStorage()
        {
            var storage = new CrateStorage();

            storage.Add(Fr8.Infrastructure.Data.Crates.Crate.FromContent("Config", new Query_DocuSign_v1.ActivityUi()));

            return storage;
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test]
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No AuthToken provided.""}",
        //    MatchType = MessageMatch.Contains
        //)]
        public async Task Query_DocuSign_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            dataDTO.ActivityDTO.AuthToken = null;

            var response = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                     configureUrl,
                     dataDTO
                );

            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test]
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""Sequence contains no elements""}",
        //    MatchType = MessageMatch.Contains
        //)]
        public async Task Query_DocuSign_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();

            var dataDTO = await HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);
            dataDTO.ActivityDTO.AuthToken = null;
            dataDTO.ActivityDTO.CrateStorage = Crate.ToDto(CreateConfiguredStorage());
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }

        [Test]
        public async Task Query_DocuSign_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Query_DocuSign_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Query_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
