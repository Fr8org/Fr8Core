using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminalDocuSign.Activities;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using FolderItem = DocuSign.eSign.Model.FolderItem;
using ListItem = Fr8Data.Control.ListItem;

namespace terminalDocuSign.Actions
{
    public class Query_DocuSign_v2 : EnhancedDocuSignActivity<Query_DocuSign_v2.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextArea IntroductionText { get; set; }

            public TextBox SearchTextFilter { get; set; }

            public DropDownList FolderFilter { get; set; }

            public DropDownList StatusFilter { get; set; }

            public ActivityUi()
            {
                IntroductionText = new TextArea
                {
                    Name = nameof(IntroductionText),
                    IsReadOnly = true,
                    Value = "<p>Search for DocuSign Envelopes where the following are true:</p><div>Envelope contains text:</div>"
                };
                SearchTextFilter = new TextBox
                {
                    Name = nameof(SearchTextFilter)
                };
                FolderFilter = new DropDownList
                {
                    Label = "Envelope is in folder:",
                    Name = nameof(FolderFilter)
                };
                StatusFilter = new DropDownList
                {
                    Label = "Envelope has status:",
                    Name = nameof(StatusFilter),
                };
                Controls.Add(IntroductionText);
                Controls.Add(SearchTextFilter);
                Controls.Add(FolderFilter);
                Controls.Add(StatusFilter);
            }
        }

        private static IEnumerable<FieldDTO> GetEnvelopeProperties()
        {
            var properties = typeof(DocuSignEnvelopeDTO).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                        .Where(x => x.CanRead && x.CanWrite);
            return properties.Select(x => new FieldDTO(x.Name, x.Name, AvailabilityType.Always)).ToArray();
        }

        private const string RunTimeCrateLabel = "DocuSign Envelope Data";

        protected override async Task Initialize(CrateSignaller crateSignaller)
        {
            var configuration = DocuSignManager.SetUp(AuthorizationToken);
            ConfigurationControls.FolderFilter.ListItems = DocuSignFolders.GetFolders(configuration)
                                                                          .Select(x => new ListItem { Key = x.Key, Value = x.Value })
                                                                          .ToList();
            ConfigurationControls.FolderFilter.ListItems.Insert(0, new ListItem { Key = "Any Folder", Value = string.Empty });
            ConfigurationControls.StatusFilter.ListItems = DocusignQuery.Statuses
                                                                        .Select(x => new ListItem { Key = x.Key, Value = x.Value })
                                                                        .ToList();
            crateSignaller.MarkAvailableAtRuntime<DocuSignEnvelopeCM_v3>(RunTimeCrateLabel)
                          .AddFields(GetEnvelopeProperties());
        }

        protected override Task Configure(CrateSignaller crateSignaller)
        {
            //No extra configuration is required
            return Task.FromResult(0);
        }

        protected override async Task RunCurrentActivity()
        {
            var configuration = DocuSignManager.SetUp(AuthorizationToken);
            var settings = GetDocusignQuery();
            var folderItems = DocuSignFolders.GetFolderItems(configuration, settings);
            CurrentPayloadStorage.Add(Crate.FromContent(RunTimeCrateLabel, new DocuSignEnvelopeCM_v3
                                                                           {
                                                                               Envelopes = folderItems.Select(ConvertFolderItemToDocuSignEnvelope).ToList()
                                                                           }));
        }

        private DocusignQuery GetDocusignQuery()
        {
            return new DocusignQuery
            {
                Folder = ConfigurationControls.FolderFilter.Value,
                Status = ConfigurationControls.StatusFilter.Value,
                SearchText = ConfigurationControls.SearchTextFilter.Value
            };
        }

        private DocuSignEnvelopeDTO ConvertFolderItemToDocuSignEnvelope(FolderItem item)
        {
            return new DocuSignEnvelopeDTO
            {
                Name = item.Name,
                CompletedDate = DateTimeHelper.Parse(item.CompletedDateTime),
                CreateDate = DateTimeHelper.Parse(item.CreatedDateTime),
                SentDate = DateTimeHelper.Parse(item.SentDateTime),
                Description = item.Description,
                TemplateId = item.TemplateId,
                Status = item.Status,
                SenderName = item.SenderName,
                Subject = item.Subject,
                EnvelopeId = item.EnvelopeId,
                SenderEmail = item.SenderEmail,
                Shared = item.Shared,
                OwnerName = item.OwnerName
            };
        }
    }
}