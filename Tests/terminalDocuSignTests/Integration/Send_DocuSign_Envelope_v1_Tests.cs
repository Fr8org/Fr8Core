using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting.Fixtures;

namespace terminalDocuSignTests
{
    [Explicit]
    public class Send_DocuSign_Envelope_v1_Tests : BaseHealthMonitorTest
    {

        public ICrateManager _crateManager;

        [SetUp]
        public void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);        
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private async Task<ActionDTO> ConfigureInitial()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_ActionDTO();
            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            return responseActionDTO;
        }

        private async Task<ActionDTO> ConfigureFollowUp()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_ActionDTO();

            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            var storage = _crateManager.GetStorage(responseActionDTO);

            SendDocuSignEnvelope_SelectFirstTemplate(storage);

            using (var updater = _crateManager.UpdateStorage(requestActionDTO))
            {
                updater.CrateStorage = storage;
            }

            return await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);
        }

        [Test]
        public async Task Configure_Initial_Test()
        {
            var responseActionDTO = await ConfigureInitial();

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var storage = _crateManager.GetStorage(responseActionDTO);

            Assert.AreEqual(3, storage.Count);
            Assert.True((storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls")));
            Assert.True((storage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Available Templates")));
            Assert.True((storage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Upstream Terminal-Provided Fields")));
        }

        [Test]
        public async Task Configure_FollowUp_Test()
        {
            var responseFollowUpActionDTO = await ConfigureFollowUp();

            // Assert FollowUp Configure result.
            Assert.NotNull(responseFollowUpActionDTO);
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage);

            var followUpStorage = _crateManager.GetStorage(responseFollowUpActionDTO);

            Assert.AreEqual(5, followUpStorage.Count);
            Assert.True((followUpStorage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "DocuSignTemplateUserDefinedFields")));
            Assert.True((followUpStorage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "DocuSignTemplateStandardFields")));
            Assert.True((followUpStorage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls")));
            Assert.True((followUpStorage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Available Templates")));
            Assert.True((followUpStorage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Upstream Terminal-Provided Fields")));
        }

/*
        [Test]
        public async Task Run_Correct_Test()
        {
            var responseFollowUpActionDTO = await ConfigureFollowUp();
            var terminalRunUrl = GetTerminalRunUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_ActionDTO();
            responseFollowUpActionDTO.AuthToken = requestActionDTO.AuthToken;
            var responsePayloadDTO = await HttpPostAsync<ActionDTO, ActionDTO>(terminalRunUrl, responseFollowUpActionDTO);
            int a = 12;

        }
        */
        private void SendDocuSignEnvelope_SelectFirstTemplate(CrateStorage curCrateStorage)
        {
            // Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
            var availableTemplatesCrateDTO = curCrateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Single(x => x.Label == "Available Templates");

            var fieldsMS = availableTemplatesCrateDTO.Content;

            // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS

            var configurationControlsCrateDTO = curCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single(x => x.Label == "Configuration_Controls");

            var controlsMS = configurationControlsCrateDTO.Content;

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var docuSignTemplateControlDTO = controlsMS.Controls.Single(x => x.Name == "target_docusign_template");
            docuSignTemplateControlDTO.Value = fieldsMS.Fields.First().Value;
        }
    }
}
