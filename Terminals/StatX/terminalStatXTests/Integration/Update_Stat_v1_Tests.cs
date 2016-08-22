using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalStatXTests.Fixtures;
using terminalStatXTests.TestTools;

namespace terminalStatXTests.Integration
{
    [Explicit]
    public class Update_Stat_v1_Tests : BaseHubIntegrationTest
    {
        private readonly AuthorizationTokenHelpers _authorizationTokenHelper;
        public Update_Stat_v1_Tests()
        {
            _authorizationTokenHelper = new AuthorizationTokenHelpers(this);
        }

        public override string TerminalName => "terminalStatX";

        [Test]
        public async Task Update_Stat_Initial_Configuration_Check_Crate_Structure()
        {
            var responseDTO = await CompleteInitialConfiguration();

            Assert.NotNull(responseDTO, "Response is null on initial configuration");
            Assert.NotNull(responseDTO.CrateStorage, "Crate storage is null on initial configuration");
            var crateStorage = Crate.FromDto(responseDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count, "Crate storage count is not equal to 1");
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "StandardConfigurationControlsCM count is not 1");

            Assert.AreEqual(4, crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls.Count, "Control count is not 4");
        }

        [Test]
        public async Task Update_Stat_FollowUp_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var responseDTO = await CompleteInitialConfiguration();
            responseDTO.AuthToken = await _authorizationTokenHelper.GetStatXAuthToken();
            var dataDTO = new Fr8DataDTO
            {
                ActivityDTO = responseDTO
            };

            responseDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            Assert.NotNull(responseDTO, "Response is null on initial configuration");
            Assert.NotNull(responseDTO.CrateStorage, "Crate storage is null on initial configuration");
            var crateStorage = Crate.FromDto(responseDTO.CrateStorage);
            Assert.AreEqual(2, crateStorage.Count, "Crate storage count is not equal to 1");
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "StandardConfigurationControlsCM count is not 1");

            Assert.AreEqual(4, crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls.Count, "Control count is not 4");

        }

        private async Task<ActivityDTO> CompleteInitialConfiguration()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = FixtureData.Update_Stat_InitialConfiguration_Fr8DataDTO();
            requestDataDTO.ActivityDTO.AuthToken = await _authorizationTokenHelper.GetStatXAuthToken();
            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
        }
    }
}
