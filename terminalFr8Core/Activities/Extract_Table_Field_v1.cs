using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using System;

namespace terminalFr8Core.Activities
{
    public class Extract_Table_Field_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("033ec734-2b2d-4671-b1e5-21bd0395c8d2"),
            Name = "Extract_Table_Field",
            Label = "Extract Table Field",
            Version = "1",
            MinPaneWidth = 330,
            NeedsAuthentication = false,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string ImmediatelyToRightKey = "Immediately to the right";
        private const string ImmediatelyToRightValue = "immediately_to_the_right";

        private const string ImmediatelyBelowKey = "Immediately below";
        private const string ImmediatelyBelowValue = "immediately_below";

        private const string AvailableCellsCrateLabel = "AvailableCells";

        private bool AreValuesSelected(DropDownList chosenCell, DropDownList position)
        {
            return !string.IsNullOrEmpty(chosenCell.selectedKey) && !string.IsNullOrEmpty(chosenCell.Value)
                   &&
                   !string.IsNullOrEmpty(position.selectedKey) && !string.IsNullOrEmpty(position.Value);
        }

        private FieldDTO GetChosenField(DropDownList chosenCell, DropDownList position)
        {
            string key;
            if (position.Value == ImmediatelyBelowValue)
            {
                key = "Value " + ImmediatelyBelowKey + " of " + chosenCell.selectedKey;
            }
            else
            {
                key = "Value " + ImmediatelyToRightKey + " of " + chosenCell.selectedKey;
            }

            return new FieldDTO(key);
        }

        private KeyValueDTO ExtractFieldFromTable(StandardTableDataCM table, DropDownList chosenCell, DropDownList position)
        {
            var field = position.Value == ImmediatelyBelowValue ? GetValueBelowSelectedCell(table, chosenCell.Value) : GetValueRightToSelectedCell(table, chosenCell.Value);

            if (field == null)
            {
                return null;
            }

            if (position.Value == ImmediatelyBelowValue)
            {
                field.Key = "Value " + ImmediatelyBelowKey + " of " + chosenCell.Value;
            }
            else
            {
                field.Key = "Value " + ImmediatelyToRightKey + " of " + chosenCell.Value;
            }

            return field;
        }

        private KeyValueDTO GetValueBelowSelectedCell(StandardTableDataCM table, string selectedCellValue)
        {
            var rows = table.Table;
            //we might have out of bounds exceptions here
            try
            {
                for (var i = 0; i < rows.Count; i++)
                {
                    var cells = rows[i].Row;
                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].Cell.Value == selectedCellValue)
                        {
                            return rows[i + 1].Row[j].Cell;
                        }
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        private KeyValueDTO GetValueRightToSelectedCell(StandardTableDataCM table, string selectedCellValue)
        {
            var rows = table.Table;
            //we might have out of bounds exceptions here
            try
            {
                for (var i = 0; i < rows.Count; i++)
                {
                    var cells = rows[i].Row;
                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].Cell.Value.Trim() == selectedCellValue.Trim())
                        {
                            return rows[i].Row[j+1].Cell;
                        }
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        private Crate FindCrateToProcess(CrateChooser crateChooser)
        {
            var selectedCrateDescription = crateChooser.CrateDescriptions.Single(c => c.Selected);
            //find crate by user selected values
            return Payload.FirstOrDefault(c => c.ManifestType.Type == selectedCrateDescription.ManifestType && c.Label == selectedCrateDescription.Label);
        }

        private void RemoveErrorText()
        {
            if (ConfigurationControls.Controls.Any(c => c.Name == "Error"))
            {
                ConfigurationControls.Controls.RemoveAll(c => c.Name == "Error");
                }
            }

        private void AddErrorText(string error)
        {
            if (ConfigurationControls.Controls.Any(c => c.Name == "Error"))
            {
                ConfigurationControls.Controls.RemoveAll(c => c.Name == "Error");
                }
            ConfigurationControls.Controls.Add(new TextBlock
                {
                    Name = "Error",
                    Value = error,
                    CssClass = "well well-lg"
                });
            }

        private void CreateControls()
        {
            var crateChooser = UiBuilder.CreateCrateChooser("TableChooser", "Select Upstream Data", true, true);
            //this cell's list items will be filled on followup configuration
            var cellDdTemplate =  new DropDownList()
            {
                Label = "Find the cell labelled",
                Name = "cellChooser",
                Required = true,
                HasRefreshButton = true,
                Source = new FieldSourceDTO()
                {
                    RequestUpstream = false,
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    Label = AvailableCellsCrateLabel
                },
                Events = new List<ControlEvent> { ControlEvent.RequestConfig }
            };

            var extractValueFromDdTemplate = new DropDownList()
            {
                Label = "and extract the value",
                Name = "extractValueFrom",
                Required = true,
                ListItems = new List<ListItem>
                {
                    new ListItem { Key = ImmediatelyToRightKey, Value = ImmediatelyToRightValue},
                    new ListItem { Key = ImmediatelyBelowKey, Value = ImmediatelyBelowValue }
                },
                Events = new List<ControlEvent> { ControlEvent.RequestConfig }
            };

            var controlList = new ControlList(new ListTemplate() { Name = "ddlb_pair", Template = { cellDdTemplate, extractValueFromDdTemplate } })
            {
                AddControlGroupButtonText = "Add Cell Extractor",
                Name = "extractor_list",
                Label = "Select fields to extract",
                NoDataMessage = "No field is selected",
                Events = new List<ControlEvent> { ControlEvent.RequestConfig }
            };

            AddControls(crateChooser, controlList);
        }

        public Extract_Table_Field_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override async Task Run()
        {
            var tableChooser = GetControl<CrateChooser>("TableChooser");
            var selectedCrateDescription = tableChooser.CrateDescriptions.FirstOrDefault(c => c.Selected);
            if (selectedCrateDescription == null)
            {
                RaiseError("No crate was selected on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }

            var crateToProcess = FindCrateToProcess(tableChooser);
            if (crateToProcess == null)
            {
                RaiseError($"Selected crate with Manifest {selectedCrateDescription.ManifestType} and label {selectedCrateDescription.Label} was not found on payload", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                return;
            }

            var extractedFieldList = new List<KeyValueDTO>();
            var chosenCellList = GetControl< ControlList>("extractor_list");
            foreach (var cGroup in chosenCellList.ControlGroups)
            {
                var chosenCell = (DropDownList)cGroup.FirstOrDefault(c => c.Name == "cellChooser");
                var position = (DropDownList)cGroup.FirstOrDefault(c => c.Name == "extractValueFrom");

                if (!AreValuesSelected(chosenCell, position))
                {
                    RaiseError("No position was selected on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                    return;
                }

                var table = crateToProcess.Get<StandardTableDataCM>();

                KeyValueDTO field = ExtractFieldFromTable(table, chosenCell, position);
                if (field == null)
                {
                    //hmm we couldn't find what we were looking for
                    RaiseError("Unable to find specified cell in StandardTableCM", ActivityErrorCode.PAYLOAD_DATA_INVALID);
                    return;
                }
                extractedFieldList.Add(field);
            }


            Payload.Add(Crate.FromContent("Extracted Field From Table", new StandardPayloadDataCM
            {
                PayloadObjects = new List<PayloadObjectDTO>() { new PayloadObjectDTO()
                {
                    PayloadObject = extractedFieldList
                } }
            }));

            Success();
        }

        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            CreateControls();
        }

        public override async Task FollowUp()
        {
            var tableChooser = GetControl<CrateChooser>("TableChooser");
            var selectedCrateDescription = tableChooser.CrateDescriptions?.FirstOrDefault(c => c.Selected);
            if (selectedCrateDescription == null)
            {
                return;
            }

            if (selectedCrateDescription.ManifestType != CrateManifestTypes.StandardTableData)
            {
                AddErrorText("This activity only supports StandardTableCM Manifests");
                return;
            }

            //let's get upstream crates to find this one
            var upstreamTableCrates = await HubCommunicator.GetCratesByDirection<StandardTableDataCM>(ActivityId, CrateDirection.Upstream);
            var selectedCrate = upstreamTableCrates.FirstOrDefault(c => c.ManifestType.Type == selectedCrateDescription.ManifestType && c.Label == selectedCrateDescription.Label);

            if (selectedCrate == null)
            {
                AddErrorText("Unable to find selected crate on an upstream activity");
                return;
            }
            RemoveErrorText();
            var table = selectedCrate.Content;
            double temp;
            var tableFields = table.Table.SelectMany(c => c.Row).Select(r => r.Cell)
                .Where(c => !string.IsNullOrEmpty(c.Value) && !double.TryParse(c.Value, out temp));

            var tempChosenCellList = GetControl<ControlList>("extractor_list");

            List<ListItem> listItems;
            if (table.FirstRowHeaders)
            {
                listItems = tableFields.Where(c => c.Key.Equals(c.Value)).Select(c => new ListItem { Key = c.Key, Value = c.Value }).ToList();
            }
            else
            {
                listItems = tableFields.Select(c => new ListItem { Key = c.Value, Value = c.Value }).ToList();
            }

            foreach (var cGroup in tempChosenCellList.ControlGroups)
            {
                var chosenCellDd = (DropDownList)cGroup.First();
                chosenCellDd.ListItems = listItems;
            }
            ((DropDownList)tempChosenCellList.TemplateContainer.Template.First()).ListItems = listItems;
         
            var chosenCellList = GetControl<ControlList>("extractor_list");
            //let's publish our data - that this will be available during runtime
            var extractedFields = new List<FieldDTO>();
            foreach (var cGroup in chosenCellList.ControlGroups)
            {
                var chosenCell = (DropDownList)cGroup.FirstOrDefault(c => c.Name == "cellChooser");
                var position = (DropDownList)cGroup.FirstOrDefault(c => c.Name == "extractValueFrom");
                if (AreValuesSelected(chosenCell, position))
                {
                    var field = GetChosenField(chosenCell, position);
                    extractedFields.Add(field);
                }
            }

            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>("Extracted Field From Table").AddFields(extractedFields);
        }
    }
}