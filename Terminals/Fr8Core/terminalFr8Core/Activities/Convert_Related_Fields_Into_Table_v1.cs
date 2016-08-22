using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;

namespace terminalFr8Core.Activities
{
    //////////////////
    /// DISABLED (needs rethinking)
    //////////////////

    public class Convert_Related_Fields_Into_Table_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("51e59b13-b164-4a4a-9a37-f528cb05e0fb"),
            Name = "Convert_Related_Fields_Into_Table",
            Label = "Convert Related Fields Into a Table",
            Version = "1",
            MinPaneWidth = 400,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string FirstIntegerRegexPattern = "\\d+";

        private IEnumerable<KeyValueDTO> ExtractFieldsContainingPrefix(IEnumerable<KeyValueDTO> fields, String prefix)
        {
            //this regex searchs for strings that start with user typed prefix value
            //and continues with a number example: "Line52_Expense", "Line4" are valid
            var matchRegex = new Regex("^(" + prefix + ")[0-9]+");
            return fields.Where(f => matchRegex.IsMatch(f.Key));
        }

        private string GetRowPrefix()
        {
            return ConfigurationControls.Controls.Single(c => c.Name == "Selected_Table_Prefix").Value;
        }


        private void CreateConfigurationControls()
        {
            var actionExplanation = new TextBlock()
            {
                Label = "This Action converts Unstructured Field data from Upstream to Standard Table data that can be more effectively manipulated.",
                Name = "Field_Explanation",
            };
            var upstreamDataChooser = new UpstreamDataChooser
            {
                Name = "Upstream_data_chooser",
                Label = "Please select data type",
                Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
            };
            var fieldSelectPrefix = new TextBox()
            {
                Label = "All field data that starts with the prefix",
                Name = "Selected_Table_Prefix",
                Required = true
            };
            var fieldExplanation = new TextBlock()
            {
                Label = "followed by an integer will be grouped into a Table Row. Example field name: 'Line3_TravelExpense",
                Name = "Field_Explanation",
            };


            AddControls(actionExplanation, upstreamDataChooser, fieldSelectPrefix, fieldExplanation);
        }


        public Convert_Related_Fields_Into_Table_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        private IEnumerable<KeyValueDTO> GetFields(IEnumerable<Crate> crates)
        {
            var fields = new List<KeyValueDTO>();

            foreach (var crate in crates)
            {
                //let's pass unknown manifests for now
                if (!crate.IsKnownManifest)
                {
                    continue;
                }

                fields.AddRange(Fr8ReflectionHelper.FindFieldsRecursive(crate.Get()));
            }

            return fields;
        }

        public override async Task Run()
        {
            var upstreamDataChooser = GetControl<UpstreamDataChooser>("Upstream_data_chooser");

            var filteredCrates = Payload.Where(s => true);

            //add filtering according to upstream data chooser
            if (upstreamDataChooser.SelectedManifest != null)
            {
                filteredCrates = filteredCrates.Where(s => s.ManifestType.Type == upstreamDataChooser.SelectedManifest);
            }
            if (upstreamDataChooser.SelectedLabel != null)
            {
                filteredCrates = filteredCrates.Where(s => s.Label == upstreamDataChooser.SelectedLabel);
            }

            var fieldList = GetFields(filteredCrates);


            if (upstreamDataChooser.SelectedFieldType != null)
            {
                //not quite sure what to do with this
                //fieldList = fieldList.Where(s => s.Tags == upstreamDataChooser.SelectedFieldType);
            }


            var prefixValue = GetRowPrefix();
            if (prefixValue == null)
            {
                RaiseError("This action can't run without a selected column prefix");
                return;
            }

            var fieldsWithPrefix = ExtractFieldsContainingPrefix(fieldList, prefixValue);


            //we get fields that match our conditions
            //and convert them to TableRowDTO
            var rows = fieldsWithPrefix
                //select each field with their row number
                //and remove prefix part
                // Line4Expense is converted to lineNumber:4 field: {Key: Expense, Value: xxx}
                .Select(f => new
                {
                    lineNumber = Int32.Parse(Regex.Match(f.Key, FirstIntegerRegexPattern).Value),
                    field = new KeyValueDTO(f.Key.Substring(f.Key.IndexOf(Regex.Match(f.Key, FirstIntegerRegexPattern).Value, StringComparison.Ordinal) + 1), f.Value)
                })
                //group by linenumber to prepare a table
                .GroupBy(r => r.lineNumber)
                .Select(r => new TableRowDTO
                {
                    Row = r.Select(f => new TableCellDTO
                    {
                        Cell = f.field
                    }).ToList()
                });

            Payload.Add("AssembledTableData", new StandardTableDataCM(false, rows.ToArray()));

            Success();
        }

        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            CreateConfigurationControls();
        }

        public override async Task FollowUp()
        {

        }
    }
}