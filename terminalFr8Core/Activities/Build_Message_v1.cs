using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class Build_Message_v1 : EnhancedTerminalActivity<Build_Message_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Build_Message",
            Label = "Build a Message",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public const string RuntimeCrateLabel = "Message Built by \"Build Message\" Activity";

        public class ActivityUi : StandardConfigurationControlsCM
        {
            
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

        public Build_Message_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        private Crate PackMessageCrate(string body = null)
        {
            return Crate.FromContent(RuntimeCrateLabel,
                                     new StandardPayloadDataCM(new FieldDTO(ActivityUI.Name.Value, body)));
        }

        private static readonly Regex FieldPlaceholdersRegex = new Regex(@"\[.*?\]");

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel, true)
                               .AddField(ActivityUI.Name.Value);

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            await Task.Factory.StartNew(RunCurrentActivityImpl);
        }

        private void RunCurrentActivityImpl()
        {
            var availableFields = ExtractAvaialbleFieldsFromPayload();
            var message = ActivityUI.Body.Value;
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
            Payload.Add(PackMessageCrate(message));
        }

        private List<FieldDTO> ExtractAvaialbleFieldsFromPayload()
        {
            var result = new List<FieldDTO>();

            result.AddRange(Payload.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.AllValues()));

            foreach (var tableCrate in Payload.CratesOfType<StandardTableDataCM>().Select(x => x.Content))
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