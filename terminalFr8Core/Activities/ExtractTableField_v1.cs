using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using AutoMapper.Internal;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;
using StructureMap;
using Hub.Interfaces;
using System.IO;

namespace terminalFr8Core.Actions
{
    public class ExtractTableField_v1 : BaseTerminalActivity
    {
        private const string ImmediatelyToRightKey = "Immediately to the right";
        private const string ImmediatelyToRightValue = "immediately_to_the_right";

        private const string ImmediatelyToRightKey = "Immediately to the right";
        private const string ImmediatelyToRightValue = "immediately_to_the_right";
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }


        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
            }

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            return Success(curPayloadDTO);
        }

        private async Task<Crate> CreateControlsCrate(ActivityDO curActivityDO)
        {
            var crateChooser = await GenerateCrateChooser(curActivityDO, "TableChooser", "Select Upstream Data", true, true, true);
            var cellDd = new DropDownList()
            {
                Label = "Find the cell labelled",
                Name = "cellChooser",
                Required = true
            };
            var extractValueFromDd = new DropDownList()
            {
                Label = "and extract the value",
                Name = "extractValueFrom",
                Required = true,
                ListItems = new List<ListItem> { new ListItem { Key = "immediately_to_the_right" }  }
            };
            return PackControlsCrate(crateChooser, cellDd);
        }
    }
}