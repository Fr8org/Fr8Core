using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Search_DocuSign_History_v1  : BaseDocuSignAction
    {
        internal class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public TextBox SearchText { get; set; }
            
            [JsonIgnore]
            public DropDownList Folder { get; set; }

            [JsonIgnore]
            public DropDownList Status { get; set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Search for DocuSign Envelopes where the following are true:</p>" +
                            "<div>Envelope contains text:</div>"
                });

                Controls.Add((SearchText = new TextBox
                {
                    Name = "SearchText",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                }));

                Controls.Add((Folder = new DropDownList
                {
                    Label = "Envelope is in folder:",
                    Name = "Folder",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                    Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "Folders")
                }));

                Controls.Add((Status = new DropDownList
                {
                    Label = "Envelope has status:",
                    Name = "Status",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                    Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "Statuses")
                }));

                Controls.Add(new RunRouteButton());
            }
        }
        
        private readonly IDocuSignFolder _docuSignFolder;
      
        public Search_DocuSign_History_v1()
        {
            _docuSignFolder = ObjectFactory.GetInstance<IDocuSignFolder>();
        }
        
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }
        
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var actionUi = new ActionUi();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {

                crateStorage.Add(PackControls(actionUi));
                crateStorage.AddRange(PackDesignTimeData(docuSignAuthDto));
            }

            await ConfigureNestedActions(curActivityDO, actionUi);
            
            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configurationControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                if (configurationControls == null)
                {
                    crateStorage.DiscardChanges();
                    return curActivityDO;
                }

                var actionUi = new ActionUi();
               
                actionUi.ClonePropertiesFrom(configurationControls);

                await ConfigureNestedActions(curActivityDO, actionUi);
                
                return curActivityDO;
            }
        }

        private async Task ConfigureNestedActions(ActivityDO curActivityDO, ActionUi actionUi)
        {
            var config = new Query_DocuSign_v1.ActionUi
            {
                Folder = {Value = actionUi.Folder.Value}, 
                Status = {Value = actionUi.Status.Value}, 
                SearchText = {Value = actionUi.SearchText.Value}
            };
            
            var template = (await FindTemplates(curActivityDO, x => x.Name == "Query_DocuSign")).FirstOrDefault();

            if (template == null)
            {
                throw new Exception("Can't find activity template: Query_DocuSign");
            }

            var storage = new CrateStorage(Data.Crates.Crate.FromContent("Config", config));

            storage.Add(PackControlsCrate(new TextArea
            {
                IsReadOnly = true,
                Label = "",
                Value = "<p>This activity is managed by the parent activity</p>"
            }));

            var activity = curActivityDO.ChildNodes.OfType<ActivityDO>().FirstOrDefault();

            if (activity == null)
            {
                activity = new ActivityDO
                {
                    ActivityTemplate = template,
                    CreateDate = DateTime.UtcNow,
                    Label = template.Label,
                    Ordering = 1,
                    ActivityTemplateId = template.Id,
                };

                curActivityDO.ChildNodes.Add(activity);
            }

            activity.CrateStorage = JsonConvert.SerializeObject(CrateManager.ToDto(storage));
        }

        private async Task<IEnumerable<ActivityTemplateDO>> FindTemplates(ActivityDO activityDO, Predicate<ActivityTemplateDO> query)
        {
            var templates = await HubCommunicator.GetActivityTemplates(activityDO, CurrentFr8UserId);
            return templates.Select(x => Mapper.Map<ActivityTemplateDO>(x)).Where(x => query(x));
        }

        private IEnumerable<Crate> PackDesignTimeData(DocuSignAuthTokenDTO authToken)
        {
            var folders = _docuSignFolder.GetSearchFolders(authToken.Email, authToken.ApiPassword);
            var fields = new List<FieldDTO>();
            
            foreach (var folder in folders)
            {
                fields.Add(new FieldDTO(folder.Name, folder.FolderId));
            }

            yield return Data.Crates.Crate.FromContent("Folders", new StandardDesignTimeFieldsCM(fields));


            yield return Data.Crates.Crate.FromContent("Statuses", new StandardDesignTimeFieldsCM(new[]
            {
                new FieldDTO("Any status", "<any>"),
                new FieldDTO("Sent", "sent"),
                new FieldDTO("Delivered", "delivered"),
                new FieldDTO("Signed", "signed"),
                new FieldDTO("Completed", "completed"),
                new FieldDTO("Declined", "declined"),
                new FieldDTO("Voided", "voided"),
                new FieldDTO("Timed Out", "timedout"),
                new FieldDTO("Authoritative Copy", "authoritativecopy"),
                new FieldDTO("Transfer Completed", "transfercompleted"),
                new FieldDTO("Template", "template"),
                new FieldDTO("Correct", "correct"),
                new FieldDTO("Created", "created"),
                new FieldDTO("Delivered", "delivered"),
                new FieldDTO("Signed", "signed"),
                new FieldDTO("Declined", "declined"),
                new FieldDTO("Completed", "completed"),
                new FieldDTO("Fax Pending", "faxpending"),
                new FieldDTO("Auto Responded", "autoresponded"),
            }));
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