using Data.Entities;
using StructureMap;
using System.Threading.Tasks;
using TerminalBase.BaseClasses;
using terminalSalesforce.Infrastructure;
using TerminalBase.Infrastructure;
using Hub.Managers;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Crates;
using Data.Control;
using Data.Interfaces.Manifests;
using System.Collections.Generic;
using System.Linq;
using System;

namespace terminalSalesforce.Actions
{
    public class Post_To_Chatter_v1 : BaseTerminalActivity
    {
        private ISalesforceManager _salesforce;

        public Post_To_Chatter_v1()
        {
            _salesforce = ObjectFactory.GetInstance<ISalesforceManager>();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            //if empty crate storage, proceed with initial config
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var chatterObjectControl = (DropDownList)GetControl(curActivityDO, "WhatKindOfChatterObject", ControlTypes.DropDownList);
            
            if(chatterObjectControl == null || string.IsNullOrEmpty(chatterObjectControl.selectedKey))
            {
                return ConfigurationRequestType.Initial;
            }         

            //proceed with follow up conifig if the above use cases are failed.
            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //prepare the available chatter objects
            var chatters = await _salesforce.GetChatters(authTokenDO);
            var availableChatters = CrateManager.CreateDesignTimeFieldsCrate("AvailableChatters", chatters.ToArray());

            //prepare configuraiton controls
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.UpdateStorage(() => curActivityDO.CrateStorage))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate, availableChatters));

                var curUpstreamFields = (await GetDesignTimeFields(curActivityDO.Id, CrateDirection.Upstream)).Fields.ToArray();
                var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);
                crateStorage.ReplaceByLabel(upstreamFieldsCrate);
            }

            return await Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            //get payload data
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            //get selected chatter object id and feed text
            var selectedChatterObjectId = ((DropDownList)GetControl(curActivityDO, "WhatKindOfChatterObject", ControlTypes.DropDownList)).Value;
            var feedText = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "FeedTextItem");

            var result = await _salesforce.PostFeedTextToChatterObject(feedText, selectedChatterObjectId, authTokenDO);

            if (!string.IsNullOrEmpty(result))
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
                {
                    var feedIdFields = new List<FieldDTO> { new FieldDTO("FeedID", result) };
                    crateStorage.Add(Crate.FromContent("Newly Created Salesforce Feed", new StandardPayloadDataCM(feedIdFields)));
                }

                return Success(payloadCrates, string.Format("Successfully posted {0} to {1}", feedText, selectedChatterObjectId));
            }

            return Error(payloadCrates, "Error when posting a Feed to Chatter", currentActivity: curActivityDO.Label, 
                currentTerminal: curActivityDO.ActivityTemplate.Terminal.Name);
        }

        private Crate CreateControlsCrate()
        {
            //DDLB for What Chatter person or group to be considered
            var whatKindOfChatterObject = new DropDownList
            {
                Name = "WhatKindOfChatterObject",
                Required = true,
                Label = "Post to which Chatter Person or Group?",
                Source = new FieldSourceDTO
                {
                    Label = "AvailableChatters",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            var feedTextItem = CreateSpecificOrUpstreamValueChooser("Feed Text", "FeedTextItem", "Upstream Terminal-Provided Fields");
            return PackControlsCrate(whatKindOfChatterObject, feedTextItem);
        }
    }
}