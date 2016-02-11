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
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
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
            var payloadStorage = Crate.GetStorage(curPayloadDTO);
            var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            if (operationsCrate == null)
            {
                return Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActionErrorCode.PAYLOAD_DATA_MISSING);
            }
            //set default loop index for initial state
            CreateLoop(curActivityDO.GetLoopId(), curPayloadDTO);
            try
            {
                if (ShouldBreakLoop(curPayloadDTO, curActivityDO))
                {
                    return SkipChildren(curPayloadDTO);
                }
                else
                    IteratePayload(curActivityDO, curPayloadDTO, 0);
            }
            catch (TerminalCodedException)
            {
                return curPayloadDTO;
            }
            return Success(curPayloadDTO);
        }

        public override async Task<PayloadDTO> ChildrenExecuted(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            int i = IncrementLoopIndex(curActivityDO.GetLoopId(), curPayloadDTO);
            try
            {
                //check if we need to end this loop
                if (ShouldBreakLoop(curPayloadDTO, curActivityDO))
                {
                    BreakLoop(curActivityDO.GetLoopId(), curPayloadDTO);
                    return Success(curPayloadDTO);
                }
                else
                    IteratePayload(curActivityDO, curPayloadDTO, i);
            }
            catch (TerminalCodedException)
            {
                return curPayloadDTO;
            }

            return ReProcessChildActions(curPayloadDTO);
        }

        //the purpose of this is to create a payload for each row in table data upon each iteration
        //introduced with FR-2246
        private void IteratePayload(ActivityDO curActivityDO, PayloadDTO curPayloadDTO, int i)
        {
            string label, type;
            var crateToProcess = FindCrateToProcess(curActivityDO, Crate.FromDto(curPayloadDTO.CrateStorage), out type, out label);

            using (var updater = Crate.UpdateStorage(curPayloadDTO))
            {
                if (i > 0)
                {
                    //remove old ones
                    updater.CrateStorage.RemoveByLabel(String.Format("row №{0} of {1}", i - 1, label));
                }

                //get row data
                var data = (crateToProcess.Get() as StandardTableDataCM).Table.ElementAt(i);
                List<FieldDTO> fields = new List<FieldDTO>();
                data.Row.ForEach(a => fields.Add(new FieldDTO(a.Cell.Key, a.Cell.Value)));
                //add row
                var crate = Data.Crates.Crate.FromContent(String.Format("row №{0} of {1}", i, label), new StandardPayloadDataCM(fields));
                updater.CrateStorage.Add(crate);
            }

        }

        private bool ShouldBreakLoop(PayloadDTO curPayloadDTO, ActivityDO curActivityDO)
        {
            var payloadStorage = Crate.GetStorage(curPayloadDTO);

            var loopId = curActivityDO.GetLoopId();
            var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            if (operationsCrate == null)
            {
                //update payload with error
                Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActionErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_INVALID);
            }
            //set default loop index for initial state

            var myLoop = operationsCrate.Loops.FirstOrDefault(l => l.Id == loopId);
            var currentLoopIndex = myLoop.Index;

            string manifestType, label;

            var crateToProcess = FindCrateToProcess(curActivityDO, payloadStorage, out manifestType, out label);

            if (crateToProcess == null)
            {
                Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActionErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + manifestType + "\" and Label: \"" + label + "\"");
            }

            Object[] dataList = null;
            //find our list data that we will iterate
            dataList = crateToProcess.IsKnownManifest ? FindFirstArray(crateToProcess.Get()) : FindFirstArray(crateToProcess.GetRaw());

            if (dataList == null)
            {
                Error(curPayloadDTO, "Unable to find a list in specified crate with Manifest Type: \"" + manifestType + "\" and Label: \"" + label + "\"", ActionErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING);
            }

            //check if we need to end this loop
            if (currentLoopIndex > dataList.Length - 1)
            {
                return true;
            }

            return false;
        }

        private Crate FindCrateToProcess(ActivityDO curActivityDO, CrateStorage payloadStorage, out string manifestType, out string label)
        {
            //get user selected design time values
            manifestType = GetSelectedCrateManifestTypeToProcess(curActivityDO);
            label = GetSelectedLabelToProcess(curActivityDO);
            var lab = label;
            //find crate by user selected values
            return payloadStorage.FirstOrDefault(c => /*c.ManifestType.Type == manifestType && */c.Label == lab);
        }

        private void BreakLoop(string loopId, PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationsData = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationsData.Loops.Single(l => l.Id == loopId).BreakSignalReceived = true;
            }
        }

        private void CreateLoop(string loopId, PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                var loopLevel = operationalState.Loops.Count(l => l.BreakSignalReceived == false);
                operationalState.Loops.Add(new OperationalStateCM.LoopStatus
                {
                    BreakSignalReceived = false,
                    Id = loopId,
                    Index = 0,
                    Level = loopLevel
                });
            }
        }

        private int IncrementLoopIndex(string loopId, PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.Loops.First(l => l.Id == loopId).Index += 1;
                return operationalState.Loops.First(l => l.Id == loopId).Index;
            }
        }

        private static object[] FindFirstArray(Object obj, int maxSearchDepth = 0)
        {
            return FindFirstArrayRecursive(obj, maxSearchDepth, 0);
        }

        private static object[] FindFirstArrayRecursive(Object obj, int maxSearchDepth, int depth)
        {
            if (maxSearchDepth != 0 && depth > maxSearchDepth)
            {
                return null;
            }

            if (obj is IEnumerable)
            {
                return ((IEnumerable)obj).OfType<Object>().ToArray();
            }
            var objType = obj.GetType();
            bool isPrimitiveType = objType.IsPrimitive || objType.IsValueType || (objType == typeof(string));

            if (!isPrimitiveType)
            {
                var objProperties = objType.GetProperties();
                foreach (var prop in objProperties)
                {
                    var result = FindFirstArrayRecursive(prop.GetValue(obj), maxSearchDepth, depth + 1);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
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
            var controlsMS = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");
            if (manifestTypeDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Loop activity can't process data without a selected Manifest Type to process");
            }
            return manifestTypeDropdown.Value;
        }

        private string GetSelectedLabelToProcess(ActivityDO curActivityDO)
        {
            var controlsMS = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
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

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                updater.CrateStorage.Add(await GetUpstreamManifestTypes(curActivityDO));
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
            var controlsMS = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");

            if (manifestTypeDropdown.Value != null)
            {
                var labelList = await GetLabelsByManifestType(curActivityDO, manifestTypeDropdown.Value);

                using (var updater = Crate.UpdateStorage(curActivityDO))
                {
                    updater.CrateStorage.RemoveByLabel("Available Labels");
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Available Labels", new StandardDesignTimeFieldsCM() { Fields = labelList }));
                }
            }

            return curActivityDO;
        }

        private async Task<Crate> GetUpstreamManifestTypes(ActivityDO curActivityDO)
        {
            var upstreamCrates = await GetCratesByDirection(curActivityDO, CrateDirection.Upstream);
            var manifestTypeOptions = upstreamCrates.GroupBy(c => c.ManifestType).Select(c => new FieldDTO(c.Key.Type, c.Key.Type));
            var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Available Manifests", manifestTypeOptions.ToArray());
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
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

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