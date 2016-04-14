using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.Infrastructure;
using terminalDocuSign.Infrastructure;
using TerminalBase.BaseClasses;
using Data.Entities;
using Utilities.Configuration.Azure;
using Data.Constants;
using Data.States;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSign.Actions
{
    public class Prepare_DocuSign_Events_For_Storage_v1 : BaseTerminalActivity
    {
        /// <summary>
        /// //For this action, both Initial and Followup configuration requests are same. Hence it returns Initial config request type always.
        /// </summary>
        /// <param name="curActivityDO"></param>
        /// <returns></returns>
        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            return await ProcessConfigurationRequest(curActivityDO, x => ConfigurationRequestType.Initial, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            /*
             * Discussed with Alexei and it is required to have empty Standard UI Control in the crate.
             * So we create a text block which informs the user that this particular aciton does not require any configuration.
             */
            var textBlock = GenerateTextBlock("Monitor All DocuSign events", "This Action doesn't require any configuration.", "well well-lg");
            var curControlsCrate = PackControlsCrate(textBlock);

            //create a Standard Event Subscription crate
            var curEventSubscriptionsCrate = CrateManager.CreateStandardEventSubscriptionsCrate("Standard Event Subscription", "DocuSign", DocuSignEventNames.GetAllEventNames());

            var envelopeCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", MT.DocuSignEnvelope_v2.ToString(), ((int)MT.DocuSignEnvelope_v2).ToString(CultureInfo.InvariantCulture), AvailabilityType.RunTime);
            //  var eventCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", MT.DocuSignEvent.ToString(), ((int)MT.DocuSignEvent).ToString(CultureInfo.InvariantCulture), AvailabilityType.RunTime);
            //var recipientCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", MT.DocuSignRecipient.ToString(), ((int)MT.DocuSignRecipient).ToString(CultureInfo.InvariantCulture), AvailabilityType.RunTime);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var authToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
                var docuSignUserCrate = Crate.FromContent("DocuSignUserCrate", new StandardPayloadDataCM(new FieldDTO("DocuSignUserEmail", authToken.Email)));
                crateStorage.Replace(new CrateStorage(curControlsCrate, curEventSubscriptionsCrate, envelopeCrate, docuSignUserCrate));
            }

            return await Task.FromResult(curActivityDO);
        }

        //public override Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        //{
        //    DocuSignAccount curDocuSignAccount = new DocuSignAccount();
        //    var curConnectProfile = curDocuSignAccount.GetDocuSignConnectProfiles();
        //    try {
        //        if (curConnectProfile.configurations != null &&
        //            !curConnectProfile.configurations.Any(config => !string.IsNullOrEmpty(config.name) && config.name.Equals("MonitorAllDocuSignEvents")))
        //        {
        //            var monitorConnectConfiguration = new DocuSign.Integrations.Client.Configuration
        //            {
        //                allowEnvelopePublish = "true",
        //                allUsers = "true",
        //                enableLog = "true",
        //                requiresAcknowledgement = "true",
        //                envelopeEvents = string.Join(",", DocuSignEventNames.GetEventsFor("Envelope")),
        //                recipientEvents = string.Join(",", DocuSignEventNames.GetEventsFor("Recipient")),
        //                name = "MonitorAllDocuSignEvents",
        //                urlToPublishTo = new Uri(new Uri(CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint")), "/terminals/terminalDocuSign/events").OriginalString
        //            };

        //            curDocuSignAccount.CreateDocuSignConnectProfile(monitorConnectConfiguration);
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        //TODO: log this exception
        //    }
        //    return Task.FromResult<ActivityDO>(curActivityDO);
        //}

        //public override Task<ActivityDO> Deactivate(ActivityDO curActivityDO)
        //{
        //    DocuSignAccount curDocuSignAccount = new DocuSignAccount();
        //    var curConnectProfile = curDocuSignAccount.GetDocuSignConnectProfiles();

        //    if (Int32.Parse(curConnectProfile.totalRecords) > 0 && curConnectProfile.configurations.Any(config => !string.IsNullOrEmpty(config.name) && config.name.Equals("MonitorAllDocuSignEvents")))
        //    {
        //        var monitorAllDocuSignEventsId = curConnectProfile.configurations.Where(config => !string.IsNullOrEmpty(config.name) && config.name.Equals("MonitorAllDocuSignEvents")).Select(s => s.connectId);
        //        foreach (var connectId in monitorAllDocuSignEventsId)
        //        {
        //            curDocuSignAccount.DeleteDocuSignConnectProfile(connectId);
        //        }
        //    }

        //    return Task.FromResult<ActivityDO>(curActivityDO);
        //}


        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curProcessPayload = await GetPayload(activityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(curProcessPayload);
            }

            var curEventReport = CrateManager.GetStorage(curProcessPayload).CrateContentsOfType<EventReportCM>().First();

            if (curEventReport.EventNames.Contains("Envelope"))
            {
                //var docuSignFields = curEventReport.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().First();
                using (var crateStorage = CrateManager.GetUpdatableStorage(curProcessPayload))
                {
                    var crate = curEventReport.EventPayload.CrateContentsOfType<DocuSignEnvelopeCM_v2>().First();
                    crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope", crate));
                }
                //var envelope = new DocuSignEnvelopeCM
                //{
                //    CompletedDate = DateTimeHelper.Parse(docuSignFields.GetValueOrDefault("CompletedDate")), //.First(field => field.Key.Equals("CompletedDate")).Value,
                //    CreateDate = DateTimeHelper.Parse(docuSignFields.GetValueOrDefault("CreateDate")),//.First(field => field.Key.Equals("CreateDate")).Value,
                //    DeliveredDate = DateTimeHelper.Parse(docuSignFields.GetValueOrDefault("DeliveredDate")),//First(field => field.Key.Equals("DeliveredDate")).Value,
                //    EnvelopeId = docuSignFields.GetValueOrDefault("EnvelopeId"),//First(field => field.Key.Equals("EnvelopeId")).Value,
                //    ExternalAccountId = docuSignFields.GetValueOrDefault("HolderEmail"),//First(field => field.Key.Equals("Email")).Value,
                //    SentDate = DateTimeHelper.Parse(docuSignFields.GetValueOrDefault("SentDate")),//First(field => field.Key.Equals("SentDate")).Value,
                //    Status = docuSignFields.GetValueOrDefault("Status"),//First(field => field.Key.Equals("Status")).Value
                //};

                //var events = new DocuSignEventCM
                //{
                //    EnvelopeId = docuSignFields.GetValueOrDefault("EnvelopeId"),//First(field => field.Key.Equals("EnvelopeId")).Value,
                //    EventId = docuSignFields.GetValueOrDefault("EventId"),//First(field => field.Key.Equals("EventId")).Value,
                //    Object = docuSignFields.GetValueOrDefault("Object"),//First(field => field.Key.Equals("Object")).Value,
                //    RecepientId = docuSignFields.GetValueOrDefault("RecipientId"),//First(field => field.Key.Equals("RecipientId")).Value,
                //    Status = docuSignFields.GetValueOrDefault("Status"),//First(field => field.Key.Equals("Status")).Value,
                //    ExternalAccountId = docuSignFields.GetValueOrDefault("HolderEmail"),//First(field => field.Key.Equals("Email")).Value
                //};

                //DocuSignRecipientCM recipientCM = null;
                //if (events.RecepientId != null)
                //{
                //    recipientCM = new DocuSignRecipientCM
                //    {
                //        RecipientId = events.RecepientId,
                //        RecipientEmail = docuSignFields.GetValueOrDefault("RecipientEmail"),
                //        Status = events.Status,
                //        Object = docuSignFields.GetValueOrDefault("Object"),
                //        EnvelopeId = docuSignFields.GetValueOrDefault("EnvelopeId"),
                //        DocuSignAccountId = docuSignFields.GetValueOrDefault("HolderEmail")
                //    };
                //}


                //using (var crateStorage = CrateManager.GetUpdatableStorage(curProcessPayload))
                //{
                //    crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope", envelope));
                //    crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Event", events));
                //    if (recipientCM != null)
                //    {
                //        crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Recipient", recipientCM));
                //    }
                //}
            }

            return Success(curProcessPayload);
        }
    }
}