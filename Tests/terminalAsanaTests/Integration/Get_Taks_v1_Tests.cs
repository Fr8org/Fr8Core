using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalAsanaTests.Integration
{
    [Explicit]
    [Category("Integration.terminalAsana")]
    class Get_Taks_v1_Tests: BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalAsana";

        private void AssertConfigurationControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(4, control.Controls.Count, "Control count is not 4");
            Assert.IsTrue(control.Controls.Where(x=> x is DropDownList).Count() == 3);
            Assert.IsTrue(control.Controls.Where(x => x is TextBlock).Count() == 1);
        }


        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task Get_Taks_v1_initial_configuration_check()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = Fixtures.FixtureData.Get_Tasks_v1_InitialConfiguration_Fr8DataDTO();
            var responseDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
            var crateStorage = Crate.FromDto(responseDTO.CrateStorage);

            AssertConfigurationControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());

            var fieldsToPayload = crateStorage.CratesOfType<CrateDescriptionCM>().ToList();


            // there should be one field of MT.AsanaTaskList and one feild of MT.StandardTableData
            Assert.AreEqual(1,fieldsToPayload.Count);

            var asanaTaskMTId = (int)MT.AsanaTaskList;
            var AsanaTaskListPayloadCount = fieldsToPayload
                    .FirstOrDefault()
                    .Content
                    .CrateDescriptions
                    .Count(x => x.ManifestId == asanaTaskMTId);

            Assert.AreEqual(AsanaTaskListPayloadCount, 1);


            var tableDataMTId = (int)MT.StandardTableData;
            var TableDataPayloadCount = fieldsToPayload
                    .FirstOrDefault()
                    .Content
                    .CrateDescriptions
                    .Count(x => x.ManifestId == asanaTaskMTId);

            Assert.AreEqual(TableDataPayloadCount, 1);
        }


        /// <summary>
        /// Validate correct crate-storage structure in followup configuration response.
        /// </summary>
        [Test]
        public async Task Get_Taks_v1_FollowUp_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();
            //var responseDTO = await CompleteInitialConfiguration();
            //responseDTO.AuthToken = FixtureData.Facebook_AuthToken();
            //var dataDTO = new Fr8DataDTO
            //{
            //    ActivityDTO = responseDTO
            //};
            ////nothing should change on followup
            //responseDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            //AssertInitialConfigurationResponse(responseDTO);
        }
    }
}
