using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class Loop_v1 : BaseTerminalAction
    {

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);
            //we used current action id to prevent mixing nested loops
            var loopIdentifierLabel = curActionDO.Id.ToString();
            var currentIndex = 0;
            using (var updater = Crate.UpdateStorage(curPayloadDTO))
            {
                var operationsData = updater.CrateStorage.CrateContentsOfType<OperationalStatusCM>(c => c.Label == loopIdentifierLabel).FirstOrDefault();
                if (operationsData == null)
                {
                    //this should be the first time our loop runs
                    operationsData = new OperationalStatusCM() { LoopIndex = 0, Break = false };
                    var operationsCrate = Crate.CreateOperationalStatusCrate(loopIdentifierLabel, operationsData);
                    updater.CrateStorage.Add(operationsCrate);
                }
                else
                {
                    operationsData.IncreaseLoopIndex();
                    currentIndex = operationsData.LoopIndex;
                }
            }

            var manifestType = GetSelectedCrateManifestTypeToProcess(curActionDO);
            var label = GetSelectedLabelToProcess(curActionDO);

            var storage = Crate.GetStorage(curPayloadDTO);
            var processingCrates = storage.Where(c => c.ManifestType.Type == manifestType && c.Label == label).ToList();

            /* should we throw exception?
            if (!processingCrates.Any())
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + manifestType + "\" and Label: \""+label+"\"");
            }
            */

            //check if we need to end this loop
            if (currentIndex > processingCrates.Count() - 1)
            {
                using (var updater = Crate.UpdateStorage(curPayloadDTO))
                {
                    var operationsData = updater.CrateStorage.CrateContentsOfType<OperationalStatusCM>(c => c.Label == loopIdentifierLabel).FirstOrDefault();
                    operationsData.BreakLoop();
                }
            }

            return curPayloadDTO;
        }

        private string GetSelectedCrateManifestTypeToProcess(ActionDO curActionDO)
        {
            //if unconfigured throw exception
            return "";
        }

        private string GetSelectedLabelToProcess(ActionDO curActionDO)
        {
            //if unconfigured throw exception
            return "";
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("CustomProcessConfiguration", new CustomProcessingConfigurationCM(true)));
            }

            return Task.FromResult(curActionDO);
        }

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }

            var fieldListControl = controlsMS.Controls.SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            if (fieldListControl.Value != null)
            {
                var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);
                userDefinedPayload.ForEach(x => x.Value = x.Key);

                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage.RemoveByLabel("ManuallyAddedPayload");
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("ManuallyAddedPayload", new StandardDesignTimeFieldsCM() { Fields = userDefinedPayload }));
                }
            }

            return Task.FromResult(curActionDO);
        }

        private Crate CreateControlsCrate()
        {
            var fieldFilterPane = new DropDownList
            {
                Label = "Fill the values for other actions",
                Name = "Selected_Fields",
                Value = "Sample Loop action"
            };

            return PackControlsCrate(fieldFilterPane);
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Initial;
            //return ConfigurationRequestType.Followup;
        }
    }
}