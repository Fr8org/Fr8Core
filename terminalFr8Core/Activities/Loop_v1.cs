using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Helpers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using Newtonsoft.Json.Linq;
using StructureMap.Diagnostics;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class Loop_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Write_To_Sql_Server",
            Label = "Write to Azure Sql Server",
            Category = ActivityCategory.Forwarders,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public override Task RunChildActivities()
        {
            JumpToActivity(ActivityId);
            return Task.FromResult(0);
        }

        protected virtual Task Validate()
        {
            var crateChooser = GetControl<CrateChooser>("Available_Crates");
            if (!crateChooser.CrateDescriptions.Any(c => c.Selected))
            {
                ValidationManager.SetError("Please select an item from the list", crateChooser);
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

        private CrateDescriptionDTO FindCrateDescriptionToProcess()
        {
            var crateChooser = GetControl<CrateChooser>("Available_Crates");
            return crateChooser.CrateDescriptions.Single(c => c.Selected);
        }


        private Crate FindCrateToProcess(CrateDescriptionDTO selectedCrateDescripiton)
        {
            return Payload.FirstOrDefault(c => c.ManifestType.Type == selectedCrateDescripiton.ManifestType && c.Label == selectedCrateDescripiton.Label);
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

        private async Task<List<FieldDTO>> GetLabelsByManifestType(string manifestType)
        {
            var upstreamCrates = await GetCratesByDirection(CrateDirection.Upstream);
            return upstreamCrates
                    .Where(c => c.ManifestType.Type == manifestType)
                    .GroupBy(c => c.Label)
                    .Select(c => new FieldDTO(c.Key, c.Key)).ToList();
        }

        private void SelectTheOnlyCrate(StandardConfigurationControlsCM controls)
        {
            var crateChooser = controls.Controls.OfType<CrateChooser>().Single();
            if (crateChooser.CrateDescriptions?.Count == 1)
            {
                crateChooser.CrateDescriptions[0].Selected = true;
            }
        }

        private async Task<Crate> CreateControlsCrate()
        {
            var crateChooser = await ControlHelper.GenerateCrateChooser(
                "Available_Crates",
                "This Loop will process the data inside of",
                true,
                requestUpstream: true,
                requestConfig: true
            );
            return PackControlsCrate(crateChooser);
        }

        public Loop_v1() : base(false)
        {
        }

        public override async Task Run()
        {
            var crateDescriptionToProcess = FindCrateDescriptionToProcess();
            var loopData = OperationalState.CallStack.GetLocalData<OperationalStateCM.LoopStatus>("Loop");
            if (loopData == null)
            {
                loopData = new OperationalStateCM.LoopStatus();
                loopData.CrateManifest = new CrateDescriptionCM(crateDescriptionToProcess);
            }
            else
            {
                loopData.Index++;
            }
            OperationalState.CallStack.StoreLocalData("Loop", loopData);
            var crateToProcess = FindCrateToProcess(crateDescriptionToProcess);
            if (crateToProcess == null)
            {
                RaiseError("This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "Unable to find any crate with Manifest Type: \"" + crateToProcess.ManifestType + "\" and Label: \"" + crateToProcess.Label + "\"");
            }

            var dataListSize = GetDataListSize(crateToProcess);
            if (dataListSize == null)
            {
                RaiseError("Unable to find a list in specified crate with Manifest Type: \"" + crateToProcess.ManifestType.Type + "\" and Label: \"" + crateToProcess.Label + "\"", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING);
            }

            if (loopData.Index >= dataListSize.Value)
            {
                SkipChildren();
                return;
            }

            Success();
        }

        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = await CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
            SelectTheOnlyCrate(Storage.FirstCrate<StandardConfigurationControlsCM>().Content);
        }

        public override async Task FollowUp()
        {
            var crateChooser = GetControl<CrateChooser>("Available_Crates");
            if (crateChooser.CrateDescriptions != null)
            {
                var selected = crateChooser.CrateDescriptions.FirstOrDefault(x => x.Selected);
                if (selected != null)
                {
                    var labelList = await GetLabelsByManifestType(selected.ManifestType);
                    Storage.RemoveByLabel("Available Labels");
                    Storage.Add(Crate.FromContent("Available Labels", new FieldDescriptionsCM { Fields = labelList }));
                    SelectTheOnlyCrate(Storage.FirstCrate<StandardConfigurationControlsCM>().Content);
                }
                else
                {
                    var configurationControlsCrate = await CreateControlsCrate();
                    Storage.Clear();
                    Storage.Add(configurationControlsCrate);
                    SelectTheOnlyCrate(Storage.FirstCrate<StandardConfigurationControlsCM>().Content);
                }
            }
        }
    }
}