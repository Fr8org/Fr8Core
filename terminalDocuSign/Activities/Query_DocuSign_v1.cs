using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Query_DocuSign_v1 : BaseDocuSignActivity
    {
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
            
            var settings = GetDocusignQuery(configurationControls);

            var payloadCm = new StandardPayloadDataCM();

            var configuration = DocuSignManager.SetUp(authTokenDO);
            DocuSignFolders.SearchDocuSign(configuration, settings, payloadCm);

            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                crateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", payloadCm));
            }

            return Success(payload);
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var configurationCrate = PackControls(new ActivityUi());

            FillFolderSource(configurationCrate, "Folder", authTokenDO);
            FillStatusSource(configurationCrate, "Status");

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(configurationCrate);
            }

            return Task.FromResult(curActivityDO);
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
    }
}