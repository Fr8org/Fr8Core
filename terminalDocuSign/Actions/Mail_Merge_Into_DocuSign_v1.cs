using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Actions
{
    public class Mail_Merge_Into_DocuSign_v1 : BasePluginAction
    {
        readonly DocuSignManager _docuSignManager;

        public Mail_Merge_Into_DocuSign_v1() : base()
        {
            _docuSignManager = new DocuSignManager();
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return null;
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private async Task<CrateDTO> CreateConfigurationControlsCrate()
        {
            var controlList = new List<ControlDefinitionDTO>();

            controlList.Add(new DropDownListControlDefinitionDTO()
            {
                Label = "1. Where is your Source Data?",
                ListItems = await GetDataSourceListItems("Table Data Generator")
            });

            controlList.Add(_docuSignManager.CreateDocuSignTemplatePicker(false, "2. Use which DocuSign Template?"));
            controlList.Add(new ButtonControlDefinisionDTO()
            {
                Label = "Continue",
                Name = "Continue"
            });

            return PackControlsCrate(controlList.ToArray());
        }

        private async Task<List<ListItem>> GetDataSourceListItems(string tag)
        {
            var httpClient = new HttpClient();
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
            + "route_nodes/available/?tag=" + tag;

            using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
            {
                var content = await response.Content.ReadAsStringAsync();
                var activityTemplate = JsonConvert.DeserializeObject<List<ActivityTemplateDTO>>(content);
                return activityTemplate.Select(at => new ListItem() { Key = at.Name, Value = at.Label }).ToList();
            }
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            CrateStorageDTO crateStrorageDTO;
            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }

            if (curActionDTO.Id > 0)
            {
                if (curActionDTO.AuthToken == null || curActionDTO.AuthToken.Token == null)
                {
                    CrateDTO configurationControlsCrate = await CreateNoAuthCrate();
                    crateStrorageDTO = AssembleCrateStorage(configurationControlsCrate);
                }
                else
                {
                    var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);
                    ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

                    //build a controls crate to render the pane
                    CrateDTO configurationControlsCrate = await CreateConfigurationControlsCrate();
                    CrateDTO templatesFieldCrate = _docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
                    crateStrorageDTO = AssembleCrateStorage(templatesFieldCrate, configurationControlsCrate);
                }
                curActionDTO.CrateStorage = crateStrorageDTO;
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }
            return curActionDTO;
        }

        private Task<CrateDTO> CreateNoAuthCrate()
        {
            var controlList = new List<ControlDefinitionDTO>();

            controlList.Add(new TextBlockControlDefinitionDTO()
            {
                Value = "This action requires authentication. Please authenticate."
            });
            return Task.FromResult(PackControlsCrate(controlList.ToArray()));
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            return ConfigurationRequestType.Initial;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            return curActionDTO;
        }
    }
}