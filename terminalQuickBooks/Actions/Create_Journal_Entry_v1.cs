using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Data.Crates;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Diagnostics;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;
using JournalEntry = Intuit.Ipp.Data.JournalEntry;

namespace terminalQuickBooks.Actions
{
    public class Create_Journal_Entry_v1 : BaseTerminalAction
    {
        private IQuickBooksIntegration _quickBooksIntegration = new QuickBooksIntegration();
        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId,
    AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
                throw new ApplicationException("No AuthToken provided.");
            var processPayload = await GetProcessPayload(curActionDO, containerId);
            return processPayload;  
        }
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (true)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
        /// <summary>
        /// Looks for first Create with Id == "Standard Design-Time" among all upcoming Actions.
        /// </summary>
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.Id != Guid.Empty)
            {
                var curUpstreamFields =
                    (await GetDesignTimeFields(curActionDO, CrateDirection.Upstream))
                    .Fields
                    .ToArray();

                // Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
                var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Involved Account", curUpstreamFields);

                //build a controls crate to render the pane
                var configurationControlsCrate = CreateControlsCrate();

                using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))
                {
                    updater.CrateStorage = AssembleCrateStorage(queryFieldsCrate, configurationControlsCrate);
                }
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }

            return curActionDO;
        }
        private Crate CreateControlsCrate()
        {
            var fieldFilterPane = new FilterPane()
            {
                Label = "Execute Actions If:",
                Name = "Selected_Filter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Involved Account",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            return PackControlsCrate(fieldFilterPane);
        }
    }
}