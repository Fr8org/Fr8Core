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

namespace terminalDocuSign.Actions
{
    public class Search_DocuSign_History_v1  : BaseTerminalAction
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
        
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDO, containerId));
        }
        
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var actionUi = new ActionUi();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                
                updater.CrateStorage.Add(PackControls(actionUi));
                updater.CrateStorage.AddRange(PackDesignTimeData(docuSignAuthDto));
            }

            await ConfigureNestedActions(curActionDO, actionUi);
            
            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                var configurationControls = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                if (configurationControls == null)
                {
                    updater.DiscardChanges();
                    return curActionDO;
                }

                var actionUi = new ActionUi();
               
                actionUi.ClonePropertiesFrom(configurationControls);

                await ConfigureNestedActions(curActionDO, actionUi);
                
                return curActionDO;
            }
        }

        private async Task ConfigureNestedActions(ActionDO curActionDO, ActionUi actionUi)
        {
            var config = new Query_DocuSign_v1.ActionUi
            {
                Folder = {Value = actionUi.Folder.Value}, 
                Status = {Value = actionUi.Status.Value}, 
                SearchText = {Value = actionUi.SearchText.Value}
            };
            
            var template = (await FindTemplates(curActionDO, x => x.Name == "Query_DocuSign")).FirstOrDefault();

            if (template == null)
            {
                throw new Exception("Can't find action template: Query_DocuSign");
            }

            var storage = new CrateStorage(Data.Crates.Crate.FromContent("Config", config));

            storage.Add(PackControlsCrate(new TextArea
            {
                IsReadOnly = true,
                Label = "",
                Value = "<p>This action is managed by the parent action</p>"
            }));

            var action = curActionDO.ChildNodes.OfType<ActionDO>().FirstOrDefault();

            if (action == null)
            {
                action = new ActionDO
                {
                    ActivityTemplate = template,
                    IsTempId = true,
                    CreateDate = DateTime.UtcNow,
                    Name = "Query DocuSign",
                    Label = template.Label,
                    Ordering = 1,
                    ActivityTemplateId = template.Id,
                };

                curActionDO.ChildNodes.Add(action);
            }

            action.CrateStorage = JsonConvert.SerializeObject(Crate.ToDto(storage));
        }

        private async Task<IEnumerable<ActivityTemplateDO>> FindTemplates(ActionDO actionDO, Predicate<ActivityTemplateDO> query)
        {
            var templates = await HubCommunicator.GetActivityTemplates(actionDO);
            return templates.Select(x => Mapper.Map<ActivityTemplateDO>(x)).Where(x => query(x));
        }

        private IEnumerable<Crate> PackDesignTimeData(DocuSignAuthTokenDTO authToken)
        {
            var folders = _docuSignFolder.GetFolders(authToken.Email, authToken.ApiPassword);
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

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}