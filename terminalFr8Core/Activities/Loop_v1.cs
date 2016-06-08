using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json.Linq;

namespace terminalFr8Core.Activities
{
    public class Loop_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Loop",
            Label = "Loop",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            Type = ActivityType.Loop,
            Tags = Tags.AggressiveReload,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public override async Task Run()
        {
            if (OperationalState == null)
            {
                RaiseError("This Action can't run without OperationalStateCM crate", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                return;
            }
            var crateDescriptionToProcess = FindCrateDescriptionToProcess();
            var loopData = OperationalState.CallStack.GetLocalData<OperationalStateCM.LoopStatus>("Loop");
            if (loopData == null)
            {
                loopData = new OperationalStateCM.LoopStatus
                {
                    CrateManifest = new CrateDescriptionCM(crateDescriptionToProcess)
                };
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
                RequestSkipChildren();
                return;
            }

            Success();
        }

        public override async Task RunChildActivities()
        {
            RequestJumpToActivity(ActivityId);
        }

        protected override Task Validate()
        {
            if (ConfigurationControls != null)
            {
                var crateChooser = GetControl<CrateChooser>("Available_Crates");

                if (!crateChooser.CrateDescriptions.Any(c => c.Selected))
                {
                    ValidationManager.SetError("Please select an item from the list", crateChooser);
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

        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = await CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
            SelectTheOnlyCrate(ConfigurationControls);
        }

        public override async Task FollowUp()
        {
            var crateChooser = GetControl<CrateChooser>("Available_Crates");
            if (crateChooser.CrateDescriptions != null)
            {
                var selected = crateChooser.CrateDescriptions.FirstOrDefault(x => x.Selected);
                if (selected != null)
                {
                    SelectTheOnlyCrate(ConfigurationControls);
                }
                else
                {
                    var configurationControlsCrate = await CreateControlsCrate();
                    Storage.Clear();
                    Storage.Add(configurationControlsCrate);
                    SelectTheOnlyCrate(ConfigurationControls);
                }
            }
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
            var crateChooser = ControlHelper.GenerateCrateChooser(
                "Available_Crates",
                "This Loop will process the data inside of",
                true,
                requestUpstream: true,
                requestConfig: true
            );
            return PackControlsCrate(crateChooser);
        }

        public Loop_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }
    }
}