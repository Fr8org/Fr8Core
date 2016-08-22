using System;
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
using System.Collections.Generic;
using Newtonsoft.Json;
using Fr8.Infrastructure.Utilities;

namespace terminalFr8Core.Activities
{
    public class Loop_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("3d5dd0c5-6702-4b59-8c18-b8e2c5955c40"),
            Name = "Loop",
            Label = "Loop",
            Version = "1",
            MinPaneWidth = 330,
            Type = ActivityType.Loop,
            Tags = Tags.AggressiveReload,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
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

            //updating iteration index
            var loopData = OperationalState.CallStack.GetLocalData<OperationalStateCM.LoopStatus>("Loop");
            if (loopData == null)
            {
                loopData = new OperationalStateCM.LoopStatus();
            }
            else
            {
                loopData.Index++;
            }
            OperationalState.CallStack.StoreLocalData("Loop", loopData);
            //end of updating

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

            PopulateRowData(crateDescriptionToProcess, loopData, crateToProcess);

            Success();
        }

        private void PopulateRowData(CrateDescriptionDTO crateDescriptionToProcess, OperationalStateCM.LoopStatus loopData, Crate crateToProcess)
        {
            string label = GetCrateName(crateDescriptionToProcess);
            Payload.RemoveUsingPredicate(a => a.Label == label && a.ManifestType == crateToProcess.ManifestType);

            if (crateDescriptionToProcess.ManifestId == (int)MT.StandardTableData)
            {
                var table = crateToProcess.Get<StandardTableDataCM>();
                var rowOfData = table.DataRows.ElementAt(loopData.Index);
                var extractedCrate = new StandardTableDataCM(false, new List<TableRowDTO>() { rowOfData });
                Payload.Add(Crate.FromContent(label, extractedCrate));
            }
            else
            {

                var cloned_crate = CloneCrateAndReplaceArrayWithASingleValue(crateToProcess, "", loopData.Index, GetCrateName(crateDescriptionToProcess));
                Payload.Add(cloned_crate);
            }
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

                if (crateChooser.CrateDescriptions != null && crateChooser.CrateDescriptions.Count > 0 && !crateChooser.CrateDescriptions.Any(c => c.Selected))
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
            var array = crateToProcess.IsKnownManifest ? Fr8ReflectionHelper.FindFirstArray(crateToProcess.Get()) : Fr8ReflectionHelper.FindFirstArray(crateToProcess.GetRaw());
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

        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            CreateControls();
            await ValidateAndHandleCrateSelection();
        }

        public override async Task FollowUp()
        {
            await ValidateAndHandleCrateSelection();
        }

        private async Task ValidateAndHandleCrateSelection()
        {
            //CrateChooser requests data only on click. So in order to autoselect a single crate - we need to make a request on our own
            var crateChooser = GetControl<CrateChooser>("Available_Crates");

            var upstreamCrates = await HubCommunicator.GetAvailableData(ActivityId, CrateDirection.Upstream, AvailabilityType.RunTime);


            if (crateChooser.CrateDescriptions == null)
            {
                crateChooser.CrateDescriptions = upstreamCrates.AvailableCrates;
            }
            else
            {
                //elements in crate chooser that are not in upstream
                var inCrateChooser = crateChooser.CrateDescriptions
                    .Where(y => !upstreamCrates.AvailableCrates.Any(z => z.ManifestId == y.ManifestId && z.SourceActivityId == y.SourceActivityId && z.Label == y.Label));
                //elements in upstream that are not in crate chooser
                var inUpstream = upstreamCrates.AvailableCrates
                  .Where(y => !crateChooser.CrateDescriptions.Any(z => z.ManifestId == y.ManifestId && z.SourceActivityId == y.SourceActivityId && z.Label == y.Label));
                if (inUpstream.Count() > 0 || inCrateChooser.Count() > 0)
                {
                    //check if selected crate is no more available
                    var old_selected = crateChooser.CrateDescriptions.FirstOrDefault(x => x.Selected);
                    if (old_selected != null)
                    {
                        var selected_in_upstream = upstreamCrates.AvailableCrates
                            .Where(a => a.Label == old_selected.Label && a.SourceActivityId == old_selected.SourceActivityId && a.ManifestId == old_selected.ManifestId)
                            .FirstOrDefault();
                        if (selected_in_upstream != null)
                        {
                            //it is available
                            selected_in_upstream.Selected = true;
                        }
                    }

                    //updated CrateChooser.CrateDescriptions
                    crateChooser.CrateDescriptions = upstreamCrates.AvailableCrates;
                }
            }

            var selected = crateChooser.CrateDescriptions.FirstOrDefault(x => x.Selected);
            if (selected != null)
            {
                SignalRowCrate(selected);
            }
            else
            {
                selected = SelectTheCrateIfThereIsOnlyOne(crateChooser);
                if (selected != null)
                    SignalRowCrate(selected);
            }

        }

        private string GetCrateName(CrateDescriptionDTO selected)
        {
            return $"Row of \"{selected.Label}\"";
        }

        private void SignalRowCrate(CrateDescriptionDTO selected)
        {
            if (selected.ManifestId == (int)MT.StandardTableData)
            {
                CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(GetCrateName(selected), true).AddFields(selected.Fields);
            }
            else
            {
                CrateSignaller.MarkAvailable(new CrateManifestType(selected.ManifestType, selected.ManifestId), GetCrateName(selected), AvailabilityType.RunTime).AddFields(selected.Fields);
            }
        }

        private CrateDescriptionDTO SelectTheCrateIfThereIsOnlyOne(CrateChooser crateChooser)
        {
            if (crateChooser.CrateDescriptions?.Count == 1)
            {
                crateChooser.CrateDescriptions[0].Selected = true;
                return crateChooser.CrateDescriptions[0];
            }
            return null;
        }

        private void CreateControls()
        {
            var crateChooser = UiBuilder.CreateCrateChooser(
                "Available_Crates",
                "This Loop will process the data inside of",
                true,
                true
                );

            AddControls(crateChooser);
        }

        public Loop_v1(ICrateManager crateManager)
                : base(crateManager)
        {
        }

        public Crate CloneCrateAndReplaceArrayWithASingleValue(Crate crateToProcess, string signalledCrateId, int index, string new_label)
        {
            var crate = CrateManager.ToDto(crateToProcess);
            var rawcrate = crate.Contents;

            var arrayProperties = rawcrate.WalkTokens().OfType<JProperty>().Where(prop => prop.Value.Type == JTokenType.Array);
            var first_array = arrayProperties.OrderBy(a => a.Path.Length).FirstOrDefault();

            if (first_array == null || first_array.Value.Type != JTokenType.Array)
                throw new ApplicationException("Manifest doesn't represent a collection of elements. Currently Loop activity is only able to process Manifest with a collection at root level. ");

            var arrayProperty = first_array.Value as JArray;
            var element = arrayProperty[index];
            arrayProperty.Clear();
            arrayProperty.Add(element);
            crate.Id = Guid.NewGuid().ToString();
            var result = CrateManager.FromDto(crate);
            result.Label = new_label;

            return result;
        }

    }


}