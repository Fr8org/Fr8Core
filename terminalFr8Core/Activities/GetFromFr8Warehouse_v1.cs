using Data.Constants;
using Data.Control;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Actions
{

    //this activity gathers all crates with a manifest, that matches the one selected in crate chooser by type
    // then it tries to find these manifests in Warehouse

    // Currently it is hard-coded to retrieve DocuSignEnvelopeCM_v2; 

    public class GetFromFr8Warehouse_v1 : EnhancedTerminalActivity<GetFromFr8Warehouse_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser UpstreamCrateChooser { get; set; }

            public ActivityUi(UiBuilder builder)
            {
                UpstreamCrateChooser = builder.CreateCrateChooser(
                        "Available_Crates",
                        "",
                        true,
                        requestConfig: true
                    );

                Controls.Add(UpstreamCrateChooser);
            }
        }

        public GetFromFr8Warehouse_v1() : base(false)
        {
            ActivityName = "Get Object From Fr8 Warehouse";
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
        }

        protected override async Task RunCurrentActivity()
        {
            var upstreamCrates = await GetCratesByDirection(CurrentActivity, Data.States.CrateDirection.Upstream);

            //TODO: select crate depending on crate chooser   
            //var crateDesription = ConfigurationControls.UpstreamCrateChooser.CrateDescriptions[0];
            //var relevantCrates = upstreamCrates.Where(a => a.ManifestType.Type == crateDesription.ManifestType);

            var relevantCrates = upstreamCrates.Where(a => a.ManifestType.Id == (int)MT.DocuSignEnvelope_v2);

            foreach (var crate in relevantCrates)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    // TODO: allow MT to return objects upcasted to ManifestType
                    var recordOfEnvelopeEvents = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM_v2>(CurrentFr8UserId, a => a.GetPrimaryKey().FirstOrDefault() == crate.Id);

                    if (recordOfEnvelopeEvents.Count > 0)
                    {
                        var last_record = recordOfEnvelopeEvents.OrderByDescending(a => a.StatusChangedDateTime).FirstOrDefault();
                        CurrentPayloadStorage.Add(Data.Crates.Crate.FromContent("RecordedEnvelope", last_record, Data.States.AvailabilityType.RunTime));
                    }
                }
            }
        }


    }
}