using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using terminalDocuSignTests.Fixtures;
using System.Collections.Generic;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Extract_Data_From_Envelopes_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(2, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableActions"));
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(2, controls.Controls.Count);
            Assert.IsTrue(controls.Controls[0] is TextArea);
            Assert.AreEqual("TextArea", controls.Controls[0].Type);
            Assert.IsTrue(controls.Controls[1] is DropDownList);
            Assert.AreEqual("FinalActionsList", controls.Controls[1].Name);
        }

        private void AddHubActivityTemplate(ActionDTO actionDTO)
        {
            AddActivityTemplate(actionDTO, HealthMonitor_FixtureData.Monitor_DocuSign_v1_ActivityTemplate_For_Solution());
            AddActivityTemplate(actionDTO, HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_ActivityTemplate_for_Solution());
        }

        private async Task<Tuple<ActionDTO, string>> GetActionDTO_WithSelectedAction()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);

            string selectedAction;

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            using (var updater = Crate.UpdateStorage(responseActionDTO))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();
                var dropDownList = (DropDownList)controls.Controls[1];

                 var availableActions = updater.CrateStorage
                    .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "AvailableActions")
                    .Single();

                dropDownList.Selected = true;
                dropDownList.selectedKey = availableActions.Fields[1].Key;
                dropDownList.Value = availableActions.Fields[1].Value;

                selectedAction = availableActions.Fields[1].Key;
            }

            return new Tuple<ActionDTO, string>(responseActionDTO, selectedAction);
        }

        [Test]
        public async void Extract_Data_From_Envelopes_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        // Validate correct crate-storage structure in follow-up configuration response.
        [Test]
        public async void Extract_Data_From_Envelopes_FollowUp_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var actionDTO = await GetActionDTO_WithSelectedAction();

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    actionDTO.Item1
                );

            var crateStorage = Crate.GetStorage(responseActionDTO);

            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());

            Assert.IsTrue(responseActionDTO.ChildrenActions.Count() > 0);
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
            

        }

        /// <summary>
        /// Select the action at run time Extract_Data_From_Envelopes_FollowUp_Configuration_Select_Action.
        /// </summary>
        [Test]
        public async void Extract_Data_From_Envelopes_FollowUp_Configuration_Select_Action()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var actionDTO = await GetActionDTO_WithSelectedAction();

            var responseActionDTO =
               await HttpPostAsync<ActionDTO, ActionDTO>(
                   configureUrl,
                   actionDTO.Item1
               );
            var crateStorage = Crate.GetStorage(responseActionDTO);

            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Monitor DocuSign Envelope Activity"));
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Send DocuSign Envelope"));
        }

        [Test]
        public async void Extract_Data_From_Envelopes_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async void Extract_Data_From_Envelopes_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
