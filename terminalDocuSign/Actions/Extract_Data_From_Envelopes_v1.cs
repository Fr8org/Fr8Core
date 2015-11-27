using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Newtonsoft.Json;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Extract_Data_From_Envelopes_v1 : BaseTerminalAction
    {
        private class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public DropDownList FinalActionsList { get; set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<img height=\"30px\" src=\"/Content/icons/web_services/DocuSign-Logo.png\">" +
							"<p>You will be asked to select a DocuSign Template.</p>" +
							"<p>Each time a related DocuSign Envelope is completed, we'll extract the data for you.</p>"
                           
                });

                Controls.Add((FinalActionsList = new DropDownList
                {
                    Name = "FinalActionsList",
                    Required = true,
					Label = "What would you like us to do with the data?",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableActions",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                }));
            }
        }

        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        public object Activate(ActionDO curDataPackage)
        {
            return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
        }

        public object Deactivate(ActionDO curDataPackage)
        {
            return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return await GetProcessPayload(actionDO, containerId);
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.AddRange(await PackSources());
            }

            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var controls = new ActionUi();
            
            controls.ClonePropertiesFrom(Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First());

            curActionDO.ChildNodes = new List<RouteNodeDO>();

			// Always use default template for solution
			const string firstTemplateName = "Monitor_DocuSign";
			var firstActionTemplate = (await FindTemplates(x => x.Name == "Monitor_DocuSign")).FirstOrDefault();

			if (firstActionTemplate == null)
			{
				throw new Exception(string.Format("ActivityTemplate {0} was not found", firstTemplateName));
			}

			var firstAction = new ActionDO
			{
				IsTempId = true,
				ActivityTemplateId = firstActionTemplate.Id,
				CrateStorage = Crate.EmptyStorageAsStr(),
				CreateDate = DateTime.UtcNow,
				Ordering = 1,
				Name = "First action"
			};

			curActionDO.ChildNodes.Add(firstAction);

            int finalActionTemplateId;

            if (int.TryParse(controls.FinalActionsList.Value, out finalActionTemplateId))
            {
                var finalAction = new ActionDO
                {
                    ActivityTemplateId = finalActionTemplateId,
                    IsTempId = true,
                    CrateStorage = Crate.EmptyStorageAsStr(),
                    CreateDate = DateTime.UtcNow,
                    Ordering = 2,
                    Name = "Final action"
                };

                curActionDO.ChildNodes.Add(finalAction);
            }
            
            return curActionDO;
        }

		private async Task<IEnumerable<ActivityTemplateDO>> FindTemplates(Predicate<ActivityTemplateDO> query)
		{
			var hubUrl = ConfigurationManager.AppSettings["CoreWebServerUrl"].TrimEnd('/') + "/route_nodes/available";
			var httpClient = new HttpClient();

			return JsonConvert.DeserializeObject<IEnumerable<ActivityTemplateCategoryDTO>>(await httpClient.GetStringAsync(hubUrl))
				.SelectMany(x => x.Activities)
				.Select(Mapper.Map<ActivityTemplateDO>).Where(x => query(x));
		}
            
        private async Task<IEnumerable<Crate>> PackSources()
        {
            var sources = new List<Crate>();

            sources.Add(Crate.CreateDesignTimeFieldsCrate("AvailableForms", new FieldDTO("key", "value")));

            var hubUrl = ConfigurationManager.AppSettings["CoreWebServerUrl"].TrimEnd('/') + "/route_nodes/available";
            var httpClient = new HttpClient();

            var catagories = JsonConvert.DeserializeObject<IEnumerable<ActivityTemplateCategoryDTO>>(await httpClient.GetStringAsync(hubUrl)).FirstOrDefault(x =>
            {
                ActivityCategory category;

                return Enum.TryParse(x.Name, out category) && category == ActivityCategory.Forwarders;
            });

            var templates = catagories != null ? catagories.Activities : new ActivityTemplateDTO[0];

            sources.Add(Crate.CreateDesignTimeFieldsCrate("AvailableActions", templates.Select(x => new FieldDTO(x.Label, x.Id.ToString())).ToArray()));

            return sources;
        }
    }
}