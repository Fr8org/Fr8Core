using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using AutoMapper.Internal;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Helpers;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Helper;
using Hub.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class Loop_v1 : BaseTerminalActivity
    {
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            var payloadStorage = CrateManager.GetStorage(curPayloadDTO);
            var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            if (operationsCrate == null)
            {
                return Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
            }

            var crateToProcess = FindCrateToProcess(curActivityDO, payloadStorage);

            if (crateToProcess == null)
            {
                Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + crateToProcess.ManifestType.Type + "\" and Label: \"" + crateToProcess.Label + "\"");
            }

            //set default loop index for initial state
            CreateLoop(curActivityDO.GetLoopId(), curPayloadDTO, crateToProcess);
            try
            {
                if (ShouldBreakLoop(curPayloadDTO, curActivityDO, crateToProcess))
                {
                    return SkipChildren(curPayloadDTO);
                }
            }
            catch (TerminalCodedException)
            {
                return curPayloadDTO;
            }
            return Success(curPayloadDTO);
        }

        protected override async Task<ICrateStorage> ValidateActivity(ActivityDO curActivityDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var controlsMS = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
                if (controlsMS != null)
                {
                    var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");

                    manifestTypeDropdown.ErrorMessage = string.IsNullOrEmpty(manifestTypeDropdown.Value) 
                        ? "Please select an item from the list." : string.Empty;

                    var labelDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Labels");

                    labelDropdown.ErrorMessage = string.IsNullOrEmpty(labelDropdown.Value) 
                        ? "Please select an item from the list." : string.Empty;
                }
            }
            return await Task.FromResult<ICrateStorage>(null);
        }

        public override async Task<PayloadDTO> ChildrenExecuted(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            var payloadStorage = CrateManager.GetStorage(curPayloadDTO);
            int i = IncrementLoopIndex(curActivityDO.GetLoopId(), curPayloadDTO);
            try
            {
                var crateToProcess = FindCrateToProcess(curActivityDO, payloadStorage);

                if (crateToProcess == null)
                {
                    Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                    throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + crateToProcess.ManifestType.Type + "\" and Label: \"" + crateToProcess.Label + "\"");
                }

                //check if we need to end this loop
                if (ShouldBreakLoop(curPayloadDTO, curActivityDO, crateToProcess))
                {
                    BreakLoop(curActivityDO.GetLoopId(), curPayloadDTO);
                    return Success(curPayloadDTO);
                }
            }
            catch (TerminalCodedException)
            {
                return curPayloadDTO;
            }

            return ReProcessChildActivities(curPayloadDTO);
        }
        /*
        //the purpose of this is to create a payload for each row in table data upon each iteration
        //introduced with FR-2246
        private void IteratePayload(ActivityDO curActivityDO, PayloadDTO curPayloadDTO, int i)
        {
            string label, type;
            var crateToProcess = FindCrateToProcess(curActivityDO, Crate.FromDto(curPayloadDTO.CrateStorage), out type, out label);

            using (var crateStorage = Crate.GetUpdatableStorage(curPayloadDTO))
            {
                if (i > 0)
                {
                    //remove old ones
                    updater.RemoveByLabel(String.Format("row №{0} of {1}", i - 1, label));
                }

                //get row data
                var data = (crateToProcess.Get() as StandardTableDataCM).Table.ElementAt(i);
                List<FieldDTO> fields = new List<FieldDTO>();
                data.Row.ForEach(a => fields.Add(new FieldDTO(a.Cell.Key, a.Cell.Value)));
                //add row
                var crate = Data.Crates.Crate.FromContent(String.Format("row №{0} of {1}", i, label), new StandardPayloadDataCM(fields));
                updater.Add(crate);
            }

        }*/

        private bool ShouldBreakLoop(PayloadDTO curPayloadDTO, ActivityDO curActivityDO, Crate crateToProcess)
        {
            var payloadStorage = CrateManager.GetStorage(curPayloadDTO);

            var loopId = curActivityDO.GetLoopId();
            var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            if (operationsCrate == null)
            {
                //update payload with error
                Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_INVALID);
            }
            //set default loop index for initial state
            var myLoop = operationsCrate.Loops.FirstOrDefault(l => l.Id == loopId);
            var currentLoopIndex = myLoop.Index;

            Object[] dataList = null;
            //find our list data that we will iterate
            dataList = crateToProcess.IsKnownManifest ? Fr8ReflectionHelper.FindFirstArray(crateToProcess.Get()) : FindFirstArray(crateToProcess.GetRaw());

            if (dataList == null)
            {
                Error(curPayloadDTO, "Unable to find a list in specified crate with Manifest Type: \"" + crateToProcess.ManifestType.Type + "\" and Label: \"" + crateToProcess.Label + "\"", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING);
            }

            //check if we need to end this loop
            if (currentLoopIndex > dataList.Length - 1)
            {
                return true;
            }

            return false;
        }

        private Crate FindCrateToProcess(ActivityDO curActivityDO, ICrateStorage payloadStorage)
        {
            //get user selected design time values
            var manifestType = GetSelectedCrateManifestTypeToProcess(curActivityDO);
            var label = GetSelectedLabelToProcess(curActivityDO);
            //find crate by user selected values
            return payloadStorage.FirstOrDefault(c => /*c.ManifestType.Type == manifestType && */c.Label == label);
        }

        private void BreakLoop(string loopId, PayloadDTO payload)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                var operationsData = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationsData.Loops.Single(l => l.Id == loopId).BreakSignalReceived = true;
            }
        }

        private void CreateLoop(string loopId, PayloadDTO payload, Crate crateToProcess)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                var operationalState = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                var loopLevel = operationalState.Loops.Count(l => l.BreakSignalReceived == false);
                operationalState.Loops.Add(new OperationalStateCM.LoopStatus
                {
                    BreakSignalReceived = false,
                    Id = loopId,
                    Index = 0,
                    Level = loopLevel,
                    Label = crateToProcess.Label,
                    CrateManifest = crateToProcess.ManifestType.Type
                });
            }
        }

        private int IncrementLoopIndex(string loopId, PayloadDTO payload)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                var operationalState = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.Loops.First(l => l.Id == loopId).Index += 1;
                return operationalState.Loops.First(l => l.Id == loopId).Index;
            }
        }

        /// <summary>
        /// Helper function that Vladimir wrote to find first array in a JToken
        /// </summary>
        /// <param name="token"></param>
        /// <param name="maxSearchDepth"></param>
        /// <returns></returns>
        private static object[] FindFirstArray(JToken token, int maxSearchDepth = 0)
        {
            return FindFirstArrayRecursive(token, maxSearchDepth, 0);
        }

        private static object[] FindFirstArrayRecursive(JToken token, int maxSearchDepth, int depth)
        {
            if (maxSearchDepth != 0 && depth > maxSearchDepth)
            {
                return null;
            }

            if (token is JArray)
            {
                return ((JArray)token).Values().OfType<object>().ToArray();
            }

            if (token is JObject)
            {
                foreach (var prop in (JObject)token)
                {
                    var result = FindFirstArrayRecursive(prop.Value, maxSearchDepth, depth + 1);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
        private string GetSelectedCrateManifestTypeToProcess(ActivityDO curActivityDO)
        {
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");
            if (manifestTypeDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Loop activity can't process data without a selected Manifest Type to process");
            }
            return manifestTypeDropdown.Value;
        }

        private string GetSelectedLabelToProcess(ActivityDO curActivityDO)
        {
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var labelDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Labels");
            if (labelDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Loop activity can't process data without a selected Label to process");
            }
            return labelDropdown.Value;
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                crateStorage.Add(await GetUpstreamManifestTypes(curActivityDO));
            }

            return curActivityDO;
        }

        private async Task<List<FieldDTO>> GetLabelsByManifestType(ActivityDO curActivityDO, string manifestType)
        {
            var upstreamCrates = await GetCratesByDirection(curActivityDO, CrateDirection.Upstream);
            return upstreamCrates
                    .Where(c => c.ManifestType.Type == manifestType)
                    .GroupBy(c => c.Label)
                    .Select(c => new FieldDTO(c.Key, c.Key)).ToList();
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");

            //refresh upstream manifest types
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("Available Manifests");
                crateStorage.Add(await GetUpstreamManifestTypes(curActivityDO));
            }

            if (manifestTypeDropdown.Value != null)
            {
                var labelList = await GetLabelsByManifestType(curActivityDO, manifestTypeDropdown.Value);

                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    crateStorage.RemoveByLabel("Available Labels");
                    crateStorage.Add(Data.Crates.Crate.FromContent("Available Labels", new StandardDesignTimeFieldsCM() { Fields = labelList }));
                }
            }


            return curActivityDO;
        }

        private async Task<Crate> GetUpstreamManifestTypes(ActivityDO curActivityDO)
        {
            var upstreamCrates = await GetCratesByDirection(curActivityDO, CrateDirection.Upstream);
            var manifestTypeOptions = upstreamCrates.GroupBy(c => c.ManifestType).Select(c => new FieldDTO(c.Key.Type, c.Key.Type));
            var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Available Manifests", manifestTypeOptions.ToArray());
            return queryFieldsCrate;
        }

        private Crate CreateControlsCrate()
        {
            var infoText = new TextBlock
            {
                Value = "This Loop will process the data inside of"
            };
            var availableManifests = new DropDownList
            {
                Label = "Crate Manifest",
                Name = "Available_Manifests",
                Value = null,
                Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                Source = new FieldSourceDTO
                {
                    Label = "Available Manifests",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            var availableLabels = new DropDownList
            {
                Label = "Crate Label",
                Name = "Available_Labels",
                Value = null,
                Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                Source = new FieldSourceDTO
                {
                    Label = "Available Labels",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            return PackControlsCrate(infoText, availableManifests, availableLabels);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var manifestTypeDropdown = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");

            if (manifestTypeDropdown == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}