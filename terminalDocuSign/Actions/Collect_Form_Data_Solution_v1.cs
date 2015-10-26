using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Collect_Form_Data_Solution_v1 : BasePluginAction
    {
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDTO, ConfigurationEvaluator);
        }

        public ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        public object Activate(ActionDTO curDataPackage)
        {
            return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
        }

        public object Deactivate(ActionDTO curDataPackage)
        {
            return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
        }

        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            return await GetProcessPayload(actionDto.ProcessId);
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }

            curActionDTO.CrateStorage.CrateDTO.Add(PackPage(new CollectFromDataSolutionUi_v1()));
            curActionDTO.CrateStorage.CrateDTO.AddRange(await PackSources());

            return curActionDTO;
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var controls = new CollectFromDataSolutionUi_v1();
            
            controls.Load(Crate.GetStandardConfigurationControls(curActionDTO.CrateStorage.CrateDTO.First(x => x.ManifestId == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_ID)));

            /*
            var form = controls.StandardFormsList.Value;
                        
            if (controls.UseStandardForm.Selected)
            {
            }

            if (controls.UseTemplate.Selected)
            {
            }

            if (controls.UseUploadedForm.Selected)
            {
            }
            */

            var finalActionTemplateId = controls.FinalActionsList.Value;
            var action = Mapper.Map<ActionDO>(curActionDTO);

            action.ChildNodes = new List<RouteNodeDO>();
            
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
                CrateStorage = JsonConvert.SerializeObject(new CrateStorageDTO()),
                CreateDate = DateTime.Now,
                Ordering = 1,
                Name = "First action"
            };

            var finalAction = new ActionDO
            {
                ActivityTemplateId = int.Parse(finalActionTemplateId),
                IsTempId = true,
                CrateStorage = JsonConvert.SerializeObject(new CrateStorageDTO()),
                CreateDate = DateTime.Now,
                Ordering = 2,
                Name = "Final action"
            };

            action.ChildNodes.Add(firstAction);
            action.ChildNodes.Add(finalAction);

            return Mapper.Map<ActionDTO>(action);
        }
        
        private async Task<IEnumerable<ActivityTemplateDO>> FindTemplates(Predicate<ActivityTemplateDO> query)
        {
            var hubUrl = ConfigurationManager.AppSettings["CoreWebServerUrl"].TrimEnd('/') + "/route_nodes/available";
            var httpClient = new HttpClient();

            return Enumerable.Where(JsonConvert
                    .DeserializeObject<IEnumerable<ActivityTemplateCategoryDTO>>(await httpClient.GetStringAsync(hubUrl))
                    .SelectMany(x => x.Activities)
                    .Select(Mapper.Map<ActivityTemplateDO>), x => query(x));
        }
        
        private async Task<IEnumerable<CrateDTO>> PackSources()
        {
            var sources = new List<CrateDTO>();

            sources.Add(Crate.CreateDesignTimeFieldsCrate("AvailableForms", new FieldDTO("key", "value")));

            var hubUrl = ConfigurationManager.AppSettings["CoreWebServerUrl"].TrimEnd('/') + "/route_nodes/available";
            var httpClient = new HttpClient();

            var catagories = JsonConvert.DeserializeObject<IEnumerable<ActivityTemplateCategoryDTO>>(await httpClient.GetStringAsync(hubUrl)).FirstOrDefault(x =>
            {
                ActivityCategory category;

                return Enum.TryParse(x.Name, out category) && category == ActivityCategory.Forwarders;
            });

            var templates = catagories != null ? catagories.Activities : new ActivityTemplateDTO[0];

            sources.Add(Crate.CreateDesignTimeFieldsCrate("AvailableActions", templates.Select(x => new FieldDTO(x.Name, x.Id.ToString())).ToArray()));

            return sources;
        }
    }
}