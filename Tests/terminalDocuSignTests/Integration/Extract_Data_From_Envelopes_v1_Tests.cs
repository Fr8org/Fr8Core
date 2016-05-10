using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using NUnit.Framework;
using HealthMonitor.Utility;
using Hub.Managers;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Extract_Data_From_Envelopes_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(2, controls.Controls.Count);
            Assert.IsTrue(controls.Controls[0] is TextArea);
            Assert.AreEqual("TextArea", controls.Controls[0].Type);
            Assert.IsTrue(controls.Controls[1] is DropDownList);
            Assert.AreEqual("FinalActionsList", controls.Controls[1].Name);
        }

        private void AddHubActivityTemplate(Fr8DataDTO dataDTO)
        {
            AddActivityTemplate(dataDTO, HealthMonitor_FixtureData.Monitor_DocuSign_v1_ActivityTemplate_For_Solution());
            AddActivityTemplate(dataDTO, HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_ActivityTemplate_for_Solution());
        }

        private async Task<Tuple<ActivityDTO, string>> GetActivityDTO_WithSelectedActivity()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(requestDataDTO);

            string selectedAction;

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();
                var dropDownList = (DropDownList)controls.Controls[1];

                 var availableActions = crateStorage
                    .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "AvailableActions")
                    .Single();

                dropDownList.Selected = true;
                dropDownList.selectedKey = availableActions.Fields[1].Key;
                dropDownList.Value = availableActions.Fields[1].Value;

                selectedAction = availableActions.Fields[1].Key;
            }
            responseActionDTO.AuthToken = requestDataDTO.ActivityDTO.AuthToken;
            return new Tuple<ActivityDTO, string>(responseActionDTO, selectedAction);
        }

        [Test]
        public async Task Extract_Data_From_Envelopes_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(requestDataDTO);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        // Validate correct crate-storage structure in follow-up configuration response.
        [Test]
        public void Extract_Data_From_Envelopes_FollowUp_Configuration_Check_Crate_Structure()
        {
                //var configureUrl = GetTerminalConfigureUrl();

                //var activityDTO = await GetActionDTO_WithSelectedAction();


                //var responseActionDTO =
                //    await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                //        configureUrl,
                //        activityDTO.Item1
                //    );
                //var crateStorage = Crate.GetStorage(responseActionDTO);

                //AssertCrateTypes(crateStorage);
                //AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());

                //Assert.IsTrue(responseActionDTO.ChildrenActions.Count() > 0);
                //Assert.NotNull(responseActionDTO);
                //Assert.NotNull(responseActionDTO.CrateStorage);

        }

        /// <summary>
        /// Select the action at run time Extract_Data_From_Envelopes_FollowUp_Configuration_Select_Action.
        /// </summary>
        [Test]
        public void Extract_Data_From_Envelopes_FollowUp_Configuration_Select_Activity()
        {
            //var configureUrl = GetTerminalConfigureUrl();
            //var activityDTO = await GetActionDTO_WithSelectedAction();

            //var responseActionDTO =
            //   await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
            //       configureUrl,
            //       activityDTO.Item1
            //   );
            //var crateStorage = Crate.GetStorage(responseActionDTO);

            //Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Monitor DocuSign Envelope Activity"));
            //Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Send DocuSign Envelope"));

       }

        [Test]
        public async Task Extract_Data_From_Envelopes_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestDataDTO = await HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(requestDataDTO);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Extract_Data_From_Envelopes_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestDataDTO = await HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(requestDataDTO);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
