using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;
using Utilities;
using FolderItem = DocuSign.eSign.Model.FolderItem;
using ListItem = Fr8Data.Control.ListItem;

namespace terminalDocuSign.Actions
{
    public class Query_DocuSign_v1 : BaseDocuSignActivity
    {
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

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);
            var configurationControls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }
            
            var configuration = DocuSignManager.SetUp(authTokenDO);

            var settings = GetDocusignQuery(configurationControls);
            var folderItems = DocuSignFolders.GetFolderItems(configuration, settings);

            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                foreach (var item in folderItems)
                {
                    crateStorage.Add(Fr8Data.Crates.Crate.FromContent(RunTimeCrateLabel, MapFolderItemToDocuSignEnvelopeCM(item)));
                }
            }

            return Success(payload);
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return Task.FromResult(curActivityDO);
            }

            var configurationCrate = PackControls(new ActivityUi());

            FillFolderSource(configurationCrate, "Folder", authTokenDO);
            FillStatusSource(configurationCrate, "Status");

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationCrate));
                crateStorage.Add(GetAvailableRunTimeTableCrate(RunTimeCrateLabel));
            }

            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("Queryable Criteria");
                return await Task.FromResult(curActivityDO);
            }
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        #region Private Methods 

        private Crate GetAvailableRunTimeTableCrate(string descriptionLabel)
        {
            var availableRunTimeCrates = Fr8Data.Crates.Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                    new CrateDescriptionDTO
                    {
                        ManifestType = MT.DocuSignEnvelope.GetEnumDisplayName(),
                        Label = descriptionLabel,
                        ManifestId = (int)MT.DocuSignEnvelope,
                        ProducedBy = "Query_DocuSign_v1"
                    }), AvailabilityType.RunTime);
            return availableRunTimeCrates;
        }

        private void FillFolderSource(Crate configurationCrate, string controlName, AuthorizationTokenDO authTokenDO)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                var conf = DocuSignManager.SetUp(authTokenDO);
                control.ListItems = DocuSignFolders.GetFolders(conf)
                    .Select(x => new ListItem() {Key = x.Key, Value = x.Value})
                    .ToList();
            }
        }

        private void FillStatusSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = DocusignQuery.Statuses
                    .Select(x => new ListItem() { Key = x.Key, Value = x.Value })
                    .ToList();
            }
        }

        private static DocusignQuery GetDocusignQuery(StandardConfigurationControlsCM configurationControls)
        {
            var actionUi = new ActivityUi();

            actionUi.ClonePropertiesFrom(configurationControls);

            var settings = new DocusignQuery();

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