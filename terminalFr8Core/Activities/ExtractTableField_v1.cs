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
using StructureMap;
using Hub.Interfaces;
using System.IO;

namespace terminalFr8Core.Activities
{
    public class ExtractTableField_v1 : BaseTerminalActivity
    {
        private const string ImmediatelyToRightKey = "Immediately to the right";
        private const string ImmediatelyToRightValue = "immediately_to_the_right";

        private const string ImmediatelyBelowKey = "Immediately below";
        private const string ImmediatelyBelowValue = "immediately_below";

        private const string ExtractedFieldsCrateLabel = "ExtractedFields";

        private const string AvailableCellsCrateLabel = "AvailableCells";
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

            return ConfigurationRequestType.Followup;
        }


        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = await CreateControlsCrate(curActivityDO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);
            var tableChooser = configControls.FindByName<CrateChooser>("TableChooser");

            var selectedCrateDescription = tableChooser.CrateDescriptions.FirstOrDefault(c => c.Selected);
            if (selectedCrateDescription == null)
            {
                return curActivityDO;
            }

            if (selectedCrateDescription.ManifestType != CrateManifestTypes.StandardTableData)
            {
                AddErrorText(curActivityDO, "This activity only supports StandardTableCM Manifests");
                return curActivityDO;
            }

            //let's get upstream crates to find this one
            var upstreamTableCrates = await HubCommunicator.GetCratesByDirection<StandardTableDataCM>(curActivityDO, CrateDirection.Upstream, CurrentFr8UserId);
            var selectedCrate = upstreamTableCrates.FirstOrDefault(c => c.ManifestType.Type == selectedCrateDescription.ManifestType && c.Label == selectedCrateDescription.Label);

            if (selectedCrate == null)
            {
                AddErrorText(curActivityDO, "Unable to find selected crate on an upstream activity");
                return curActivityDO;
            }

            RemoveErrorText(curActivityDO);

            

            var table = selectedCrate.Content;
            double temp;
            var tableFields = table.Table.SelectMany(c => c.Row).Select(r => r.Cell)
                .Select(c => new FieldDTO(c.Value, c.Value))
                .Where(c => !string.IsNullOrEmpty(c.Value) && !double.TryParse(c.Value, out temp));
            
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var tempChosenCellList = GetConfigurationControls(crateStorage).FindByName<ControlList>("extractor_list");
                //TODO do this with a more efficient way
                //all dropdowns should use same data
                var listItems = tableFields.Select(c => new ListItem { Key = c.Key, Value = c.Value }).ToList();
                foreach (var cGroup in tempChosenCellList.ControlGroups)
                {
                    var chosenCellDd = (DropDownList)cGroup.First();
                    chosenCellDd.ListItems = listItems;
                }
                ((DropDownList) tempChosenCellList.TemplateContainer.Template.First()).ListItems = listItems;
                /*
                crateStorage.RemoveByLabel(AvailableCellsCrateLabel);
                crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate(AvailableCellsCrateLabel, AvailabilityType.Configuration, tableFields.ToArray()));
                */
            }


            var chosenCellList = configControls.FindByName<ControlList>("extractor_list");


            //let's publish our data - that this will be available during runtime
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var extractedFields = new List<FieldDTO>();
                foreach (var cGroup in chosenCellList.ControlGroups)
                {
                    var chosenCell = (DropDownList)cGroup.FirstOrDefault(c => c.Name=="cellChooser");
                    var position = (DropDownList)cGroup.FirstOrDefault(c => c.Name == "extractValueFrom");
                    if (AreValuesSelected(chosenCell, position))
                    {
                        extractedFields.Add(GetChosenField(chosenCell, position));
                    }
                }

            crateStorage.RemoveByLabel(ExtractedFieldsCrateLabel);
                var crate = Crate.FromContent(ExtractedFieldsCrateLabel, new FieldDescriptionsCM() { Fields = extractedFields });
                crate.Availability = AvailabilityType.RunTime;
                crateStorage.Add(crate);
            }
            

            return curActivityDO;
        }

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

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            var curPayloadStorage = CrateManager.GetStorage(curPayloadDTO);


            var configControls = GetConfigurationControls(curActivityDO);
            var tableChooser = configControls.FindByName<CrateChooser>("TableChooser");
            var selectedCrateDescription = tableChooser.CrateDescriptions.FirstOrDefault(c => c.Selected);
            if (selectedCrateDescription == null)
            {
                return Error(curPayloadDTO, "No crate was selected on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            var crateToProcess = FindCrateToProcess(tableChooser, curPayloadStorage);
            if (crateToProcess == null)
            {
                return Error(curPayloadDTO, $"Selected crate with Manifest {selectedCrateDescription.ManifestType} and label {selectedCrateDescription.Label} was not found on payload", ActivityErrorCode.PAYLOAD_DATA_MISSING);
            }

            
            using (var updater = CrateManager.GetUpdatableStorage(curPayloadDTO))
            {
                var extractedFieldList = new List<FieldDTO>();
                var chosenCellList = configControls.FindByName<ControlList>("extractor_list");
                foreach (var cGroup in chosenCellList.ControlGroups)
                {
                    var chosenCell = (DropDownList)cGroup.FirstOrDefault(c => c.Name == "cellChooser");
                    var position = (DropDownList)cGroup.FirstOrDefault(c => c.Name == "extractValueFrom");

                    if (!AreValuesSelected(chosenCell, position))
                    {
                        return Error(curPayloadDTO, "No position was selected on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                    }

                    var table = crateToProcess.Get<StandardTableDataCM>();

                    FieldDTO field = ExtractFieldFromTable(table, chosenCell, position);
                    if (field == null)
                    {
                        //hmm we couldn't find what we were looking for
                        return Error(curPayloadDTO, "Unable to find specified cell in StandardTableCM", ActivityErrorCode.PAYLOAD_DATA_INVALID);
                    }
                    extractedFieldList.Add(field);
                }


                updater.Add(Crate.FromContent("Extracted Field From Table", new StandardPayloadDataCM
                {
                    PayloadObjects = new List<PayloadObjectDTO>() { new PayloadObjectDTO()
                    {
                        PayloadObject = extractedFieldList
                    } }
                }));
            }

            return Success(curPayloadDTO);
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

        private Crate FindCrateToProcess(CrateChooser crateChooser, ICrateStorage payloadStorage)
        {
            var selectedCrateDescription = crateChooser.CrateDescriptions.Single(c => c.Selected);
            //find crate by user selected values
            return payloadStorage.FirstOrDefault(c => c.ManifestType.Type == selectedCrateDescription.ManifestType && c.Label == selectedCrateDescription.Label);
        }

        private void RemoveErrorText(ActivityDO curActivityDO)
        {
            using (var updater = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configControls = GetConfigurationControls(updater);
                if (configControls.Controls.Any(c => c.Name == "Error"))
                {
                    configControls.Controls.RemoveAll(c => c.Name == "Error");
                }
            }
        }

        private void AddErrorText(ActivityDO curActivityDO, string error)
        {
            using (var updater = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configControls = GetConfigurationControls(updater);
                if (configControls.Controls.Any(c => c.Name == "Error"))
                {
                    configControls.Controls.RemoveAll(c => c.Name == "Error");
                }

                configControls.Controls.Add(new TextBlock
                {
                    ErrorMessage = error,
                    Name = "Error",
                    Value = error,
                    CssClass = "well well-lg"
                });
            }
        }

        private async Task<Crate> CreateControlsCrate(ActivityDO curActivityDO)
        {
            var crateChooser = await GenerateCrateChooser(curActivityDO, "TableChooser", "Select Upstream Data", true, true, true);
            //this cell's list items will be filled on followup configuration
            var cellDdTemplate =  new DropDownList()
            {
                Label = "Find the cell labelled",
                Name = "cellChooser",
                Required = true,
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
    }
}