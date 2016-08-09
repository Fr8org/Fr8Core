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
using Fr8.Infrastructure.Utilities;
using Newtonsoft.Json;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using FolderItem = DocuSign.eSign.Model.FolderItem;
using System;

namespace terminalDocuSign.Activities
{
    public class Query_DocuSign_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("62CB1D64-1A94-483C-A577-DA514F5D0CB0"),
            Name = "Query_DocuSign",
            Label = "Query DocuSign",
            Version = "1",
            NeedsAuthentication = true,
            MinPaneWidth = 380,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
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

        public override async Task Run()
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

        public override Task Initialize()
        {
            Storage.Clear();

            AddControls(new ActivityUi().Controls);

            FillFolderSource("Folder");
            FillStatusSource("Status");

            Storage.Add(GetAvailableRunTimeTableCrate(RunTimeCrateLabel));
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
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
                        ProducedBy = "Query_DocuSign_v1",
                        Availability = AvailabilityType.RunTime
                    }));
            return availableRunTimeCrates;
        }

        private void FillFolderSource(string controlName)
        {
            var control = ConfigurationControls.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                var conf = DocuSignManager.SetUp(AuthorizationToken);
                control.ListItems = DocuSignFolders.GetFolders(conf)
                    .Select(x => new ListItem() { Key = x.Key, Value = x.Value })
                    .ToList();
            }
        }

        private void FillStatusSource(string controlName)
        {
            var control = ConfigurationControls.FindByNameNested<DropDownList>(controlName);
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