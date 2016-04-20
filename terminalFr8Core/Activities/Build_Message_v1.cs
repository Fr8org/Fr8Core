using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Hub.Managers;
using Data.Interfaces.Manifests;
using Data.Control;
using Data.States;
using System.Text.RegularExpressions;
using Data.Constants;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class Build_Message_v1 : EnhancedTerminalActivity<Build_Message_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public const string RuntimeCrateLabel = "Build Message";
            public TextBox Name { get; set; }

            public BuildMessageAppender Body { get; set; }

            public ActivityUi()
            {
                Name = new TextBox
                {
                    Label = "Name",
                    Name = nameof(Name),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Body = new BuildMessageAppender
                {
                    Label = "Body",
                    Name = nameof(Body),
                    IsReadOnly = false,
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true,
                        AvailabilityType = AvailabilityType.RunTime
                    }
                };
                Controls = new List<ControlDefinitionDTO> { Name, Body };
            }
        }

        public Build_Message_v1() : base(false)
        {
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            //var upstreamFields = await ExtractUpstreamFields();
            //ConfigurationControls.AvailableFields.ListItems = upstreamFields.OrderBy(x => x.Key).Select(x => new ListItem { Key = x.Key, Value = x.Value }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<FieldDescriptionsCM>(ActivityUi.RuntimeCrateLabel);
            CurrentActivityStorage.ReplaceByLabel(PackMessageCrate());
        }

        private Crate<FieldDescriptionsCM> PackMessageCrate()
        {
            return Crate<FieldDescriptionsCM>.FromContent(ActivityUi.RuntimeCrateLabel,
                                                          new FieldDescriptionsCM(new FieldDTO(ConfigurationControls.Name.Value, ConfigurationControls.Name.Value)), AvailabilityType.RunTime);
        }

        private Crate<StandardPayloadDataCM> PackMessageCrate(string body)
        {
            return Crate<StandardPayloadDataCM>.FromContent(ActivityUi.RuntimeCrateLabel,
                                                          new StandardPayloadDataCM(new FieldDTO(ConfigurationControls.Name.Value, body)), AvailabilityType.RunTime);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            runtimeCrateManager.MarkAvailableAtRuntime<FieldDescriptionsCM>(ActivityUi.RuntimeCrateLabel);
            CurrentActivityStorage.ReplaceByLabel(PackMessageCrate());
        }

        private static readonly Regex FieldPlaceholdersRegex = new Regex(@"\[.*?\]");

        protected override async Task RunCurrentActivity()
        {
            await Task.Factory.StartNew(RunCurrentActivityImpl);
        }

        private void RunCurrentActivityImpl()
        {
            var availableFields = ExtractAvaialbleFieldsFromPayload();
            var message = ConfigurationControls.Body.Value;
            if (availableFields.Count > 0 && !string.IsNullOrEmpty(message))
            {
                var messageBodyBuilder = new StringBuilder(message);
                //We sort placeholders in reverse order so we can replace them starting from the last that won't break any previous match indices
                var foundPlaceholders = FieldPlaceholdersRegex.Matches(message).Cast<Match>().OrderByDescending(x => x.Index).ToArray();
                foreach (var placeholder in foundPlaceholders)
                {
                    var replaceWith = availableFields.FirstOrDefault(x => string.Equals(x.Key, placeholder.Value.TrimStart('[').TrimEnd(']')));
                    if (replaceWith != null)
                    {
                        messageBodyBuilder.Replace(placeholder.Value, replaceWith.Value, placeholder.Index, placeholder.Value.Length);
                    }
                }
                message = messageBodyBuilder.ToString();
            }
            CurrentPayloadStorage.Add(PackMessageCrate(message));
        }

        private List<FieldDTO> ExtractAvaialbleFieldsFromPayload()
        {
            var result = new List<FieldDTO>();
            result.AddRange(CurrentPayloadStorage.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.AllValues()));
            result.AddRange(CurrentPayloadStorage.CratesOfType<FieldDescriptionsCM>().SelectMany(x => x.Content.Fields));
            foreach (var tableCrate in CurrentPayloadStorage.CratesOfType<StandardTableDataCM>().Select(x => x.Content))
            {
                //We should take first row of data only if there is at least one data row. We never take header row if it exists
                var rowToTake = tableCrate.FirstRowHeaders
                                    ? tableCrate.Table.Count > 1
                                          ? 1
                                          : -1
                                    : tableCrate.Table.Count > 0
                                            ? 0
                                            : -1;
                if (rowToTake == -1)
                {
                    continue;
                }
                result.AddRange(tableCrate.Table[rowToTake].Row.Select(x => x.Cell));
            }
            return result;
        }
    }
}