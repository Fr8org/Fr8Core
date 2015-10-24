using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xaml;
using AutoMapper;
using AutoMapper.Mappers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Collect_Form_Data_Solution_v1 : BasePluginAction
    {
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (ValidateAuthentication(curActionDTO, AuthenticationMode.InternalMode))
            {
                return await ProcessConfigurationRequest(curActionDTO, ConfigurationEvaluator);
            }

            return curActionDTO;
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

           curActionDTO.CrateStorage.CrateDTO.Add(PackControlsCrate(LoadConfigurationControls("terminalDocuSign.Actions.Collect_From_Data_Solution_v1.xaml").ToArray()));
           curActionDTO.CrateStorage.CrateDTO.AddRange(await PackSources());
           
            return curActionDTO;
        }

        protected override Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var configurationFields = Crate.GetConfigurationControls(Mapper.Map<ActionDO>(curActionDTO));

            return Task.FromResult(curActionDTO);
        }

        private async Task<IEnumerable<CrateDTO>> PackSources()
        {
            var sources = new List<CrateDTO>();
            
            sources.Add(Crate.CreateDesignTimeFieldsCrate("AvailableForms", new FieldDTO("key", "value")));

            var hubUrl = ConfigurationManager.AppSettings["CoreWebServerUrl"].TrimEnd('/') + "/route_nodes/available";
            var httpClient = new HttpClient()
            {
                
            };

            var catagories = JsonConvert.DeserializeObject<IEnumerable<ActivityTemplateCategoryDTO>>(await httpClient.GetStringAsync(hubUrl)).FirstOrDefault(x =>
            {
                ActivityCategory category;

                return Enum.TryParse(x.Name, out category) && category == ActivityCategory.Forwarders;
            });

            var templates = catagories != null ? catagories.Activities : new ActivityTemplateDTO[0];

            sources.Add(Crate.CreateDesignTimeFieldsCrate("AvailableActions", templates.Select(x => new FieldDTO(x.Name, x.Id.ToString())).ToArray()));

            return sources;
        }


        private IEnumerable<ControlDefinitionDTO> LoadConfigurationControls(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                return ((PageDTO)XamlServices.Load(stream)).Children;
            }
        }
    }
}