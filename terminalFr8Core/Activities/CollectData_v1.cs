using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Actions
{
    public class CollectData_v1 : BaseTerminalActivity
    {
        public const string CollectionControlsLabel = "Collection";
        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateInitialControlsCrate();
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
            }

            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);

            var controlContainer = configControls.FindByName<MetaControlContainer>("control_container");
            if (!controlContainer.MetaDescriptions.Any())
            {
                //TODO add error label
                return curActivityDO;
            }

            var storage = CrateManager.GetStorage(curActivityDO);
            //user might have pressed submit button on Collection UI
            var collectionControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();
            if (collectionControls != null)
            {
                var submitButton = collectionControls.FindByName<Button>("submit_button");
                if (submitButton.Clicked)
                {
                    //we need to start the process
                    
                }
            }

            using (var curStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var updateButton = GetConfigurationControls(storage).FindByName<Button>("update_launcher");
                if (updateButton.Clicked)
                {
                    updateButton.Clicked = false;
                    curStorage.RemoveByLabel(CollectionControlsLabel);
                    curStorage.Add(CreateCollectionControlsCrate(controlContainer));
                }
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            return Success(curPayloadDTO);
        }

        protected Crate CreateCollectionControlsCrate(MetaControlContainer controlContainer)
        {
            var generatedConfigControls = controlContainer.CreateControls();
            //let's add a submit button here
            var submitButton = new Button
            {
                CssClass = "float-right mt30 btn btn-default",
                Label = "Submit",
                Name = "submit_button",
                Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onClick", "requestConfig")
                    }
            };
            generatedConfigControls.Add(submitButton);
            return Crate<StandardConfigurationControlsCM>.FromContent(CollectionControlsLabel, new StandardConfigurationControlsCM(generatedConfigControls.ToArray()), AvailabilityType.Configuration);
        }

        protected Crate CreateInitialControlsCrate()
        {
            var infoText = new TextBlock()
            {
                Value = "Construct a Launcher that gathers information from users and passes it to another Plan",
                Name = "info_text"
            };

            var cc = new MetaControlContainer()
            {
                Label = "Please insert your desired controls below",
                Name = "control_container"
            };

            var updateButton = new Button
            {
                CssClass = "float-right mt30 btn btn-default",
                Label = "Update Launcher",
                Name = "update_launcher",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            };

            return PackControlsCrate(infoText, cc, updateButton);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}