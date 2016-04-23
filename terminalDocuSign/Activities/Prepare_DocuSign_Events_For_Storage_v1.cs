using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Activities
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

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var authToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
                var docuSignUserCrate = Crate.FromContent("DocuSignUserCrate", new StandardPayloadDataCM(new FieldDTO("DocuSignUserEmail", authToken.Email)));
                crateStorage.Replace(new CrateStorage(curControlsCrate, curEventSubscriptionsCrate, envelopeCrate, docuSignUserCrate));
            }

            return await Task.FromResult(curActivityDO);
        }


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
                using (var crateStorage = CrateManager.GetUpdatableStorage(curProcessPayload))
                {
                    var crate = curEventReport.EventPayload.CrateContentsOfType<DocuSignEnvelopeCM_v2>().First();
                    crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope", crate));
                }
            }

            return Success(curProcessPayload);
        }
    }
}