using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginBase.Infrastructure;
using StructureMap;
using PluginBase;
using PluginBase.BaseClasses;
using Core.Interfaces;
using Core.Services;
using Core.StructureMap;
using Data.States.Templates;
using Data.Interfaces.ManifestSchemas;
using pluginSendGrid.Infrastructure;
using Data.Interfaces;
using pluginSendGrid.Services;
using System.Threading.Tasks;

namespace pluginSendGrid.Actions
{
    public class SendEmailViaSendGrid_v1 : BasePluginAction
    {
        protected IEmailPackager _emailPackager;//moved the EmailPackager ObjectFactory here since the basepluginAction will be called by others and the dependency is defiend in pluginsendGrid

        // protected override void SetupServices()
        // {
        //     base.SetupServices();
        //     _emailPackager = ObjectFactory.GetInstance<IEmailPackager>();
        // }
        
        //================================================================================
        //General Methods (every Action class has these)

        //maybe want to return the full Action here
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await ProcessConfigurationRequest(curActionDTO, EvaluateReceivedRequest);
        }

        //this entire function gets passed as a delegate to the main processing code in the base class
        //currently many actions have two stages of configuration, and this method determines which stage should be applied
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;
            else
                return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }
            curActionDTO.CrateStorage.CrateDTO.Add(CreateControlsCrate());
            //commeted first for the avialble datafields for the dropdownlist to wait for the Nested Controls to be implemented
            //curActionDTO.CrateStorage.CrateDTO.Add(await GetAvailableDataFields(curActionDTO));
            return curActionDTO;
        }

        private CrateDTO CreateControlsCrate()
        {
            ControlDefinitionDTO[] controls = 
            {
                new ControlDefinitionDTO(ControlTypes.TextBox)
                {
                    Label = "Recipient Email Address",
                    Name = "Recipient_Email_Address",
                    Required = false
                },
                new ControlDefinitionDTO(ControlTypes.TextBox)
                {
                    Label = "Email Subject",
                    Name = "Email_Subject",
                    Required = false
                },
                new ControlDefinitionDTO(ControlTypes.TextBox)
                {
                    Label = "Email Body",
                    Name = "Email_Body",
                    Required = false
                }
            };

            return _crate.CreateStandardConfigurationControlsCrate("Send Grid", controls);
        }

        private async Task<CrateDTO> GetAvailableDataFields(ActionDTO curActionDTO)
        {
            CrateDTO crateDTO = null;
            ActionDO curActionDO =  _action.MapFromDTO(curActionDTO);
            var curUpstreamFields = (await GetDesignTimeFields(curActionDO.Id, GetCrateDirection.Upstream)).Fields.ToArray();

            if (curUpstreamFields.Length == 0)
            {
                crateDTO = GetTextBoxControlForDisplayingError("Error_NoUpstreamLists",
                         "No Upstream fr8 Lists Were Found.");
                curActionDTO.CurrentView = "Error_NoUpstreamLists";
            }
            else
            {
                crateDTO = _crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);
            }

            return crateDTO;
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            //not currently any requirements that need attention at FollowupConfigurationResponse
            return curActionDTO;
        }

        public object Activate(ActionDO curActionDO)
        {
            //not currently any requirements that need attention at Activation Time
            return curActionDO;
        }

        public object Deactivate(ActionDO curActionDO)
        {
            return curActionDO;
        }

        public object Execute(ActionDataPackageDTO curActionDataPackage)
        {
            var mailerDO = AutoMapper.Mapper.Map<IMailerDO>(curActionDataPackage.PayloadDTO);

            _emailPackager.Send(mailerDO);

            return true;
        }
    }
}