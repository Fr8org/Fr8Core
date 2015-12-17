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
    public class Loop_v1 : BaseTerminalAction
    {
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);
            var payloadStorage = Crate.GetStorage(curPayloadDTO);

            var loopId = curActionDO.Id.ToString();
            var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            if (operationsCrate == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "This Action can't run without OperationalStateCM crate");
            }
            //set default loop index for initial state
            var currentLoopIndex = 0;
            var myLoop = operationsCrate.Loops.FirstOrDefault(l => l.Id == loopId);
            if (myLoop == null)
            {
                CreateLoop(curActionDO.Id.ToString(), curPayloadDTO);
            }
            else
            {
                currentLoopIndex = IncrementLoopIndex(curActionDO.Id.ToString(), curPayloadDTO);
            }

            //get user selected design time values
            var manifestType = GetSelectedCrateManifestTypeToProcess(curActionDO);
            var label = GetSelectedLabelToProcess(curActionDO);
            
            //find crate by user selected values
            var crateToProcess = payloadStorage.FirstOrDefault(c => /*c.ManifestType.Type == manifestType && */c.Label == label);

            if (crateToProcess == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + manifestType + "\" and Label: \""+label+"\"");
            }

            Object[] dataList = null;
            //find our list data that we will iterate
            dataList = crateToProcess.IsKnownManifest ? FindFirstArray(crateToProcess.Get()) : FindFirstArray(crateToProcess.GetRaw());

            if (dataList == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find a list in specified crate with Manifest Type: \"" + manifestType + "\" and Label: \"" + label + "\"");
            }

            //check if we need to end this loop
            if (currentLoopIndex > dataList.Length - 1)
            {
                BreakLoop(curActionDO.Id.ToString(), curPayloadDTO);
            }

            return curPayloadDTO;
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
                return ((IEnumerable) obj).OfType<Object>().ToArray();
            }
            var objType = obj.GetType();
            bool isPrimitiveType = objType.IsPrimitive || objType.IsValueType || (objType == typeof (string));

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
        private string GetSelectedCrateManifestTypeToProcess(ActionDO curActionDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");
            if (manifestTypeDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Loop action can't process data without a selected Manifest Type to process");
            }
            return manifestTypeDropdown.Value;
        }

        private string GetSelectedLabelToProcess(ActionDO curActionDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var labelDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Labels");
            if (labelDropdown.Value == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Loop action can't process data without a selected Label to process");
            }
            return labelDropdown.Value;
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                updater.CrateStorage.Add(await GetUpstreamManifestTypes(curActionDO));
            }

            return curActionDO;
        }

        private async Task<List<FieldDTO>> GetLabelsByManifestType(ActionDO curActionDO, string manifestType)
        {
            var upstreamCrates = await GetCratesByDirection(curActionDO, CrateDirection.Upstream);
            return upstreamCrates
                    .Where(c => c.ManifestType.Type == manifestType)
                    .GroupBy(c => c.Label)
                    .Select(c => new FieldDTO(c.Key, c.Key)).ToList();
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_Manifests");

            if (manifestTypeDropdown.Value != null)
            {
                var labelList = await GetLabelsByManifestType(curActionDO, manifestTypeDropdown.Value);

                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage.RemoveByLabel("Available Labels");
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Available Labels", new StandardDesignTimeFieldsCM() { Fields = labelList }));
                }
            }

            return curActionDO;
        }

        private async Task<Crate> GetUpstreamManifestTypes(ActionDO curActionDO)
        {
            var upstreamCrates = await GetCratesByDirection(curActionDO, CrateDirection.Upstream);
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
                Events = new List<ControlEvent>{ new ControlEvent("onChange", "requestConfig") },
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
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                Source = new FieldSourceDTO
                {
                    Label = "Available Labels",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            return PackControlsCrate(infoText, availableManifests, availableLabels);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

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