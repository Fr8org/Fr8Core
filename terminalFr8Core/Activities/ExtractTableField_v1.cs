using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Activities
{
    public class ExtractTableField_v1 : BaseTerminalActivity
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

        private const string ImmediatelyToRightKey = "Immediately to the right";
        private const string ImmediatelyToRightValue = "immediately_to_the_right";

        private const string ImmediatelyBelowKey = "Immediately below";
        private const string ImmediatelyBelowValue = "immediately_below";

        private const string ExtractedFieldsCrateLabel = "ExtractedFields";

        private const string AvailableCellsCrateLabel = "AvailableCells";

        private bool AreValuesSelected(DropDownList chosenCell, DropDownList position)
        {
            return !string.IsNullOrEmpty(chosenCell.selectedKey) && !string.IsNullOrEmpty(chosenCell.Value)
                   &&
                   !string.IsNullOrEmpty(position.selectedKey) && !string.IsNullOrEmpty(position.Value);
        }

        private FieldDTO GetChosenField(DropDownList chosenCell, DropDownList position)
        {
            var key = "";
            if (position.Value == ImmediatelyBelowValue)
            {
                key = "Value " + ImmediatelyBelowKey + " of " + chosenCell.selectedKey;
            }
            else
            {
                key = "Value " + ImmediatelyToRightKey + " of " + chosenCell.selectedKey;
            }

            return new FieldDTO(key, chosenCell.selectedKey);
        }

        private FieldDTO ExtractFieldFromTable(StandardTableDataCM table, DropDownList chosenCell, DropDownList position)
        {
            FieldDTO field = position.Value == ImmediatelyBelowValue ?
                GetValueBelowSelectedCell(table, chosenCell.Value) : GetValueRightToSelectedCell(table, chosenCell.Value);

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

        private FieldDTO GetValueBelowSelectedCell(StandardTableDataCM table, string selectedCellValue)
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

        private FieldDTO GetValueRightToSelectedCell(StandardTableDataCM table, string selectedCellValue)
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

        private async Task<Crate> CreateControlsCrate()
        {
            var crateChooser = await ControlHelper.GenerateCrateChooser("TableChooser", "Select Upstream Data", true, true, true);
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

            return PackControlsCrate(crateChooser, controlList);
        }

        public ExtractTableField_v1() : base(false)
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

            var extractedFieldList = new List<FieldDTO>();
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

                FieldDTO field = ExtractFieldFromTable(table, chosenCell, position);
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
            var configurationControlsCrate = await CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
        }

        public override async Task FollowUp()
        {
            var tableChooser = GetControl<CrateChooser>("TableChooser");
            var selectedCrateDescription = tableChooser.CrateDescriptions.FirstOrDefault(c => c.Selected);
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
            var upstreamTableCrates = await GetCratesByDirection<StandardTableDataCM>(CrateDirection.Upstream);
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
                .Select(c => new FieldDTO(c.Value, c.Value))
                .Where(c => !string.IsNullOrEmpty(c.Value) && !double.TryParse(c.Value, out temp));

            var tempChosenCellList = GetControl<ControlList>("extractor_list");
            //TODO do this with a more efficient way
            //all dropdowns should use same data
            var listItems = tableFields.Select(c => new ListItem { Key = c.Key, Value = c.Value }).ToList();
            foreach (var cGroup in tempChosenCellList.ControlGroups)
            {
                var chosenCellDd = (DropDownList)cGroup.First();
                chosenCellDd.ListItems = listItems;
            }
            ((DropDownList)tempChosenCellList.TemplateContainer.Template.First()).ListItems = listItems;
            /*
            crateStorage.RemoveByLabel(AvailableCellsCrateLabel);
            crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate(AvailableCellsCrateLabel, AvailabilityType.Configuration, tableFields.ToArray()));
            */
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
                    field.Availability = AvailabilityType.RunTime;
                    extractedFields.Add(field);
                }
            }
            Storage.RemoveByLabel(ExtractedFieldsCrateLabel);
            var crate = Crate.FromContent(ExtractedFieldsCrateLabel, new FieldDescriptionsCM() { Fields = extractedFields });
            crate.Availability = AvailabilityType.RunTime;
            Storage.Add(crate);
        }
    }
}