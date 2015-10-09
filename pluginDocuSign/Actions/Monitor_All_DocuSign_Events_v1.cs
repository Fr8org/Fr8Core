using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
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
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(curActionDTO, AuthenticationMode.InternalMode);
                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            //For this action, both Initial and Followup configuration requests are same. Hence it returns Initial config request type always.
            return await ProcessConfigurationRequest(curActionDTO, dto => ConfigurationRequestType.Initial);
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            /*
             * Discussed with Alexei and it is required to have empty Standard Configuration Control in the crate.
             * So we create a text block which informs the user that this particular aciton does not require any configuration.
             */
            var textBlock = new TextBlockFieldDTO()
            {
                Label = "Monitor All DocuSign events",
                Value = "This Action doesn't require any configuration.",
                cssClass = "well well-lg"
            };
            var curControlsCrate = PackControlsCrate(textBlock);

            //create a Standard Event Subscription crate
            var curEventSubscriptionsCrate = _crate.CreateStandardEventSubscriptionsCrate("Standard Event Subscription", DocuSignEventNames.GetAllEventNames());

            //update crate storage with standard event subscription crate
            curActionDTO.CrateStorage = new CrateStorageDTO()
            {
                CrateDTO = new List<CrateDTO> { curControlsCrate, curEventSubscriptionsCrate }
            };

            /*
             * Note: We should not call Activate at the time of Configuration. For this action, it may be valid use case.
             * Because this particular action will be used internally, it would be easy to execute the Process directly.
             */
            await Activate(curActionDTO);

            return await Task.FromResult(curActionDTO);
        }

        public Task<object> Activate(ActionDTO curActionDTO)
        {
            DocuSignAccount curDocuSignAccount = new DocuSignAccount();
            var curConnectProfile = curDocuSignAccount.GetDocuSignConnectProfiles();

            if (curConnectProfile.configurations != null &&
                !curConnectProfile.configurations.Any(config => config.name.Equals("MonitorAllDocuSignEvents")))
            {
                var monitorConnectConfiguration = new DocuSign.Integrations.Client.Configuration
                {
                    allowEnvelopePublish = "true",
                    allUsers = "true",
                    enableLog = "true",
                    requiresAcknowledgement = "true",
                    envelopeEvents = string.Join(",", DocuSignEventNames.GetEventsFor("Envelope")),
                    recipientEvents = string.Join(",", DocuSignEventNames.GetEventsFor("Recipient")),
                    name = "MonitorAllDocuSignEvents",
                    urlToPublishTo =
                        Regex.Match(ConfigurationManager.AppSettings["EventWebServerUrl"], @"(\w+://\w+:\d+)").Value +
                        "/events?dockyard_plugin=pluginDocuSign&version=1"
                };

                curDocuSignAccount.CreateDocuSignConnectProfile(monitorConnectConfiguration);
            }

            return Task.FromResult((object)true);
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