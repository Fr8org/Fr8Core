using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pluginAzureSqlServer.Infrastructure;
using pluginAzureSqlServer.Services;
using PluginBase.Infrastructure;
using StructureMap;
using PluginBase;
using PluginBase.BaseClasses;
using Core.Interfaces;
using Core.Services;
using Core.StructureMap;
using Data.States.Templates;
using Data.Interfaces.ManifestSchemas;

namespace pluginSendGrid.Actions
{
    public class Send_Email_Via_SendGrid_v1 : BasePluginAction
    {
         private IAction _action;
        private ICrate _crate;

        public Write_To_Sql_Server_v1()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }




        //================================================================================
        //General Methods (every Action class has these)

        //maybe want to return the full Action here
        public ActionDTO Configure(ActionDTO curActionDTO)
        {
            return ProcessConfigurationRequest(curActionDTO, EvaluateReceivedRequest);
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

        protected override ActionDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }
            var crateControls = CreateControlsCrate();
            var crateDataFields = CreateDataFields();
            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
            return curActionDTO;
        }

        private CrateDTO CreateControlsCrate() { 

            // "[{ type: 'textField', name: 'connection_string', required: true, value: '', fieldLabel: 'SQL Connection String' }]"
            var control = new FieldDefinitionDTO()
            {
                    FieldLabel = "SQL Connection String",
                    Type = "textField",
                    Name = "connection_string",
                    Required = true,
                    Events = new List<FieldEvent>() {new FieldEvent("onChange", "requestConfig")}
            };
            return PackControlsCrate(control);
        }

         private CrateDTO CreateDataFields() { 

            var control = new FieldDefinitionDTO()
            {
                    FieldLabel = "SQL Connection String",
                    Type = "textField",
                    Name = "connection_string",
                    Required = true,
                    Events = new List<FieldEvent>() {new FieldEvent("onChange", "requestConfig")}
            };
            return PackControlsCrate(control);
        }

        protected override ActionDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
         //not currently any requirements that need attention at FollowupConfigurationResponse
            return null;
        }

        public object Activate(ActionDO curActionDO)
        {
            //not currently any requirements that need attention at Activation Time
            return null;
        }

        public object Deactivate(ActionDO curActionDO)
        {
            return null;
        }

        public object Execute(ActionDataPackageDTO curActionDataPackage)
        {
            //var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDataPackage.ActionDTO);
            //var curCommandArgs = PrepareSQLWrite(curActionDO);
            //var dbService = new DbService();

            //dbService.WriteCommand(curCommandArgs);

            return true;
        }
    }
}