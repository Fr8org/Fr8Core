using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Helpers;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json.Linq;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class Loop_v1 : BaseTerminalActivity
    {
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);

            using (var payloadStorage = CrateManager.UpdateStorage(() => curPayloadDTO.CrateStorage))
            {
                var operationsCrate = payloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();

                if (operationsCrate == null)
                {
                    return Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                }

                var crateDescriptionToProcess = FindCrateDescriptionToProcess(curActivityDO);

                var loopData = operationsCrate.CallStack.GetLocalData<OperationalStateCM.LoopStatus>("Loop");

                if (loopData == null)
                {
                    loopData = new OperationalStateCM.LoopStatus();
                    loopData.CrateManifest = new CrateDescriptionCM(crateDescriptionToProcess);
                }
                else
                {
                    loopData.Index++;
                }

                operationsCrate.CallStack.StoreLocalData("Loop", loopData);

                var crateToProcess = FindCrateToProcess(crateDescriptionToProcess, payloadStorage);
                if (crateToProcess == null)
                {
                    Error(curPayloadDTO, "This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                    throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + crateToProcess.ManifestType + "\" and Label: \"" + crateToProcess.Label + "\"");
                }

                var dataListSize = GetDataListSize(crateToProcess);

                if (dataListSize == null)
                {
                    Error(curPayloadDTO, "Unable to find a list in specified crate with Manifest Type: \"" + crateToProcess.ManifestType.Type + "\" and Label: \"" + crateToProcess.Label + "\"", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                    throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING);
                }

                if (loopData.Index >= dataListSize.Value)
                {
                    SkipChildren(payloadStorage);
                    return curPayloadDTO;
                }
            }

            return Success(curPayloadDTO);
        }

        public override async Task<PayloadDTO> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);

            return JumpToActivity(curPayloadDTO, curActivityDO.Id);
        }

        public override Task ValidateActivity(ActivityDO curActivityDO, ICrateStorage crateStorage, ValidationManager validationManager)
        {
            var controlsMS = GetConfigurationControls(crateStorage);

            if (controlsMS != null)
            {
                var crateChooser = GetControl<CrateChooser>(controlsMS, "Available_Crates");

                if (!crateChooser.CrateDescriptions.Any(c => c.Selected))
                {
                    validationManager.SetError("Please select an item from the list", crateChooser);
                }
            }

            return Task.FromResult(0);
        }

        internal static int? GetDataListSize(Crate crateToProcess)
        {
            var tableData = crateToProcess.ManifestType.Id == (int)MT.StandardTableData ? crateToProcess.Get<StandardTableDataCM>() : null;
            if (tableData != null)
            {
                return tableData.FirstRowHeaders ? Math.Max(0, tableData.Table.Count - 1) : tableData.Table.Count;
            }
            var array = crateToProcess.IsKnownManifest ? Fr8ReflectionHelper.FindFirstArray(crateToProcess.Get()) : FindFirstArray(crateToProcess.GetRaw());
            return array?.Length;
        }

        private CrateDescriptionDTO FindCrateDescriptionToProcess(ActivityDO curActivityDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);
            var crateChooser = (CrateChooser)configControls.Controls.Single(c => c.Name == "Available_Crates");
            return crateChooser.CrateDescriptions.Single(c => c.Selected);
        }


        private Crate FindCrateToProcess(CrateDescriptionDTO selectedCrateDescripiton, ICrateStorage payloadStorage)
        {
            return payloadStorage.FirstOrDefault(c => c.ManifestType.Type == selectedCrateDescripiton.ManifestType && c.Label == selectedCrateDescripiton.Label);
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

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = await CreateControlsCrate(curActivityDO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));

                // TODO: remove, FR-2691.
                // crateStorage.Add(await GetUpstreamManifestTypes(curActivityDO));
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
            var crateChooser = (CrateChooser)controlsMS.Controls.Single(x => x.Type == ControlTypes.CrateChooser && x.Name == "Available_Crates");

            //refresh upstream manifest types

            // TODO: remove, FR-2691.
            // using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            // {
            //     crateStorage.RemoveByLabel("Available Manifests");
            //     crateStorage.Add(await GetUpstreamManifestTypes(curActivityDO));
            // }



            if (crateChooser.CrateDescriptions != null)
            {
                var selected = crateChooser.CrateDescriptions.FirstOrDefault(x => x.Selected);
                if (selected != null)
                {
                    var labelList = await GetLabelsByManifestType(curActivityDO, selected.ManifestType);

                    using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                    {
                        crateStorage.RemoveByLabel("Available Labels");
                        crateStorage.Add(Data.Crates.Crate.FromContent("Available Labels",
                            new FieldDescriptionsCM() { Fields = labelList }));
                    }
                }
                else
                {
                    var configurationControlsCrate = await CreateControlsCrate(curActivityDO);

                    using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                    {
                        crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                    }
                }
            }

            return curActivityDO;
        }

        private async Task<Crate> CreateControlsCrate(ActivityDO curActivityDO)
        {
            var crateChooser = await GenerateCrateChooser(
                curActivityDO,
                "Available_Crates",
                "This Loop will process the data inside of",
                true,
                requestUpstream: true,
                requestConfig: true
            );
            return PackControlsCrate(crateChooser);
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

            var manifestTypeDropdown = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.CrateChooser && x.Name == "Available_Crates");

            if (manifestTypeDropdown == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}