using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;      
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Activities;

namespace terminalAsanaTests.Integration
{
    [Explicit]
    //[Ignore]
    class Post_Comment_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalAsana";

        private void AssertConfigurationControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(4, control.Controls.Count, "Control count is not 4");
            Assert.IsTrue(control.Controls.Count(x => x.GetType() == typeof(DropDownList)) == 3);
            Assert.IsTrue(control.Controls.Count(x => x.GetType() == typeof(TextSource)) == 1);
        }

        private void AssertInitialConfigurationResponse(ActivityDTO responseDTO)
        {
            Assert.NotNull(responseDTO, "Response is null on initial configuration");
            Assert.NotNull(responseDTO.CrateStorage, "Crate storage is null on initial configuration");
            var crateStorage = Crate.FromDto(responseDTO.CrateStorage);
            AssertConfigurationControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        private async Task<ActivityDTO> CompleteInitialConfiguration()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = Fixtures.FixtureData.Post_Comment_v1_InitialConfiguration_Fr8DataDTO();
            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response. OAuth Token already should be present in ActivityDTO
        /// </summary>
        [Test, Ignore("Being fixed in FR-6098")]
        public async Task Post_Comment_v1_initial_configuration_check()
        {
            var responseDTO = await CompleteInitialConfiguration();

            AssertInitialConfigurationResponse(responseDTO);
        }


        /// <summary>
        /// Validate correct crate-storage structure in followup configuration response.
        /// same test as for Get_Task activity
        /// </summary>
        [Test, Ignore("Being fixed in FR-6098")]
        public async Task Post_Comment_v1_FollowUp_Configuration_Check_Crate_Structure()
        {
            // it is integration test so it will be oooho loooong.
            var configureUrl = GetTerminalConfigureUrl();
            var responseDTO = await CompleteInitialConfiguration();
            
            var token = Fixtures.FixtureData.SampleAuthorizationToken();

            var payload = Mapper.Map<ActivityPayload>(responseDTO);
            var crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var ddlb = crates.Controls.Find(x => x.Name.Equals("Workspaces")) as DropDownList;
            ddlb.Value = ddlb.ListItems[0].Value;
            ddlb.selectedKey = ddlb.ListItems[0].Key;

            responseDTO = Mapper.Map<ActivityDTO>(payload);
            
            var tokenDTO = Mapper.Map<AuthorizationTokenDTO>(token);
            responseDTO.AuthToken = tokenDTO;

            var dataDTO = new Fr8DataDTO
            {
                ActivityDTO = responseDTO,
            };
            
            responseDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            // we should get at least one project if test account not empty.
            payload = Mapper.Map<ActivityPayload>(responseDTO);
            crates = payload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            ddlb = crates.Controls.Find(x => x.Name.Equals("Projects")) as DropDownList;
            Assert.IsTrue(ddlb.ListItems.Count > 0);

            AssertInitialConfigurationResponse(responseDTO);
        }
    }
}
