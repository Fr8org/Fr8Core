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
using terminalDocuSign.Activities;
using terminalDocuSign.DataTransferObjects;
using TerminalBase.Infrastructure;
using terminalDocuSign.Services;

namespace terminalDocuSign.Activities
{
    public class Search_DocuSign_History_v1  : BaseDocuSignActivity
    {
        internal class ActivityUi : StandardConfigurationControlsCM
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
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                                          });

                Controls.Add(Folder = new DropDownList
                {
                    Label = "Envelope is in folder:",
                    Name = "Folder",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                    Source = null
                                      });

                Controls.Add(Status = new DropDownList
                {
                    Label = "Envelope has status:",
                    Name = "Status",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                    Source = null
                                      });

                Controls.Add(new RunPlanButton());
            }
        }


        protected override string ActivityUserFriendlyName => "Search DocuSign History";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }
        
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            var actionUi = new ActivityUi();
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);           
            var configurationCrate = PackControls(actionUi);
            //commented out by FR-2400
            //_docuSignManager.FillFolderSource(configurationCrate, "Folder", docuSignAuthDTO);
            //_docuSignManager.FillStatusSource(configurationCrate, "Status");

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(configurationCrate);
            }

            await ConfigureNestedActivities(curActivityDO, actionUi);
            
            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configurationControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                if (configurationControls == null)
                {
                    crateStorage.DiscardChanges();
                    return curActivityDO;
                }

                var actionUi = new ActivityUi();
               
                actionUi.ClonePropertiesFrom(configurationControls);

                await ConfigureNestedActivities(curActivityDO, actionUi);
                
                return curActivityDO;
            }
        }

        private async Task ConfigureNestedActivities(ActivityDO curActivityDO, ActivityUi actionUi)
        {
            var config = new Query_DocuSign_v1.ActivityUi
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
            var templates = await HubCommunicator.GetActivityTemplates(CurrentFr8UserId);
            return templates.Select(x => Mapper.Map<ActivityTemplateDO>(x)).Where(x => query(x));
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