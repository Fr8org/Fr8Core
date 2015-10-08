using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using pluginDocuSign.Infrastructure;
using Configuration = DocuSign.Integrations.Client.Configuration;

namespace pluginDocuSign.Actions
{
    public class Monitor_All_DocuSign_Events_v1 : BasePluginAction
    {
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            //For this action, both Initial and Followup configuration requests are same. Hence it returns Initial config request type always.
            return await ProcessConfigurationRequest(curActionDTO, dto => ConfigurationRequestType.Initial);
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            //create a Standard Event Subscription crate
            var curEventSubscriptionCrate = _crate.CreateStandardEventSubscriptionsCrate("Standard Event Subscription", DocuSignEventNames.GetAllEventNames());

            //update crate storage with standard event subscription crate
            curActionDTO.CrateStorage = new CrateStorageDTO()
            {
                CrateDTO = new List<CrateDTO> { curEventSubscriptionCrate }
            };

            return await Task.FromResult(curActionDTO);
        }

        public object Activate(ActionDTO curActionDTO)
        {
            DocuSignAccount curDocuSignAccount = new DocuSignAccount();
            var curConnectProfile = curDocuSignAccount.GetDocuSignConnectProfiles();

            var monitorConnectConfiguration = new DocuSign.Integrations.Client.Configuration
            {
                envelopeEvents = string.Join(",", DocuSignEventNames.GetEventsFor("Envelope")),
                recipientEvents = string.Join(",", DocuSignEventNames.GetEventsFor("Recipient")),
                name = "MonitorAllDocuSignEvents",
                urlToPublishTo =
                    Regex.Match(ConfigurationManager.AppSettings["EventWebServerUrl"], @"(\w+://\w+:\d+)").Value +
                    "/events?dockyard_plugin=pluginDocuSign&version=1"
            };

            if (Int32.Parse(curConnectProfile.totalRecords) > 0 &&
                curConnectProfile.configurations.Any(config => config.name.Equals("MonitorAllDocuSignEvents")))
            {
                curDocuSignAccount.UpdateDocuSignConnectProfile(monitorConnectConfiguration);
            }
            else
            {
                curDocuSignAccount.CreateDocuSignConnectProfile(monitorConnectConfiguration);
            }

            return true;
        }

        public object Deactivate(ActionDTO curDataPackage)
        {
            DocuSignAccount curDocuSignAccount = new DocuSignAccount();
            var curConnectProfile = curDocuSignAccount.GetDocuSignConnectProfiles();

            if (Int32.Parse(curConnectProfile.totalRecords) > 0 && curConnectProfile.configurations.Any(config => config.name.Equals("MonitorAllDocuSignEvents")))
            {
                curDocuSignAccount.DeleteDocuSignConnectProfile("MonitorAllDocuSignEvents");
            }

            return true;
        }

        public async Task<PayloadDTO> Execute(ActionDTO actionDto)
        {
            return null;
        }
    }
}