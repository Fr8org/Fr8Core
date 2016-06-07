using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Newtonsoft.Json;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using Utilities;
using FolderItem = DocuSign.eSign.Model.FolderItem;
using ListItem = Fr8Data.Control.ListItem;

namespace terminalDocuSign.Activities
{
    public class Query_DocuSign_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Query_DocuSign",
            Label = "Query DocuSign",
            Version = "1",
            Category = ActivityCategory.Receivers,
            NeedsAuthentication = true,
            MinPaneWidth = 380,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RunTimeCrateLabel = "DocuSign Envelope Data";

        public class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public TextBox SearchText { get; set; }

            [JsonIgnore]
            public DropDownList Folder { get; set; }

            [JsonIgnore]
            public DropDownList Status { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Search for DocuSign Envelopes where the following are true:</p>" +
                            "<div>Envelope contains text:</div>"
                });

                Controls.Add(SearchText = new TextBox
                {
                    Name = "SearchText",
                });

                Controls.Add(Folder = new DropDownList
                {
                    Label = "Envelope is in folder:",
                    Name = "Folder",
                    Source = null
                });

                Controls.Add(Status = new DropDownList
                {
                    Label = "Envelope has status:",
                    Name = "Status",
                    Source = null
                });
            }
        }

        protected override string ActivityUserFriendlyName => "Query DocuSign";

        public Query_DocuSign_v1(ICrateManager crateManager, IDocuSignManager docuSignManager) 
            : base(crateManager, docuSignManager)
        {
        }

        protected override async Task RunDS()
        {
            var configuration = DocuSignManager.SetUp(AuthorizationToken);
            var settings = GetDocusignQuery();
            var folderItems = DocuSignFolders.GetFolderItems(configuration, settings);
            foreach (var item in folderItems)
            {
                Payload.Add(Crate.FromContent(RunTimeCrateLabel, MapFolderItemToDocuSignEnvelopeCM(item)));
            }
            Success();
        }

        protected override Task InitializeDS()
        {
            var configurationCrate = PackControls(new ActivityUi());
            FillFolderSource(configurationCrate, "Folder");
            FillStatusSource(configurationCrate, "Status");
            Storage.Clear();
            Storage.Add(configurationCrate);
            Storage.Add(GetAvailableRunTimeTableCrate(RunTimeCrateLabel));
            return Task.FromResult(0);
        }

        protected override async Task FollowUpDS()
        {
            Storage.RemoveByLabel("Queryable Criteria");
        }

        #region Private Methods 

        private Crate GetAvailableRunTimeTableCrate(string descriptionLabel)
        {
            var availableRunTimeCrates = Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                    new CrateDescriptionDTO
                    {
                        ManifestType = MT.DocuSignEnvelope.GetEnumDisplayName(),
                        Label = descriptionLabel,
                        ManifestId = (int)MT.DocuSignEnvelope,
                        ProducedBy = "Query_DocuSign_v1"
                    }), AvailabilityType.RunTime);
            return availableRunTimeCrates;
        }

        private void FillFolderSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                var conf = DocuSignManager.SetUp(AuthorizationToken);
                control.ListItems = DocuSignFolders.GetFolders(conf)
                    .Select(x => new ListItem() { Key = x.Key, Value = x.Value })
                    .ToList();
            }
        }

        private void FillStatusSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = DocuSignQuery.Statuses
                    .Select(x => new ListItem() { Key = x.Key, Value = x.Value })
                    .ToList();
            }
        }

        private DocuSignQuery GetDocusignQuery()
        {
            var actionUi = new ActivityUi();

            actionUi.ClonePropertiesFrom(ConfigurationControls);

            var settings = new DocuSignQuery();

            settings.Folder = actionUi.Folder.Value;
            settings.Status = actionUi.Status.Value;
            settings.SearchText = actionUi.SearchText.Value;

            return settings;
        }

        private DocuSignEnvelopeCM MapFolderItemToDocuSignEnvelopeCM(FolderItem folderItem)
        {
            return new DocuSignEnvelopeCM()
            {
                EnvelopeId = folderItem.EnvelopeId,

                Name = folderItem.Name,
                Subject = folderItem.Subject,
                OwnerName = folderItem.OwnerName,
                SenderName = folderItem.SenderName,
                SenderEmail = folderItem.SenderEmail,
                Shared = folderItem.Shared,
                Status = folderItem.Status,
                CompletedDate = DateTimeHelper.Parse(folderItem.CompletedDateTime),
                CreateDate = DateTimeHelper.Parse(folderItem.CreatedDateTime),
                SentDate = DateTimeHelper.Parse(folderItem.SentDateTime)
            };
        }

        #endregion
    }
}