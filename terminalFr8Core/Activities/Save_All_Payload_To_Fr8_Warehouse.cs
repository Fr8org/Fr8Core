using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using StructureMap;
using System;
using Fr8.Infrastructure.Data.Constants;

namespace terminalFr8Core.Activities
{
    public class Save_All_Payload_To_Fr8_Warehouse : ExplicitTerminalActivity
    {
        private readonly IContainer _container;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("0228092d-213b-435a-8041-b80ac0022824"),
            Name = "Save_All_Payload_To_Fr8_Warehouse",
            Label = "Save All Payload To Fr8 Warehouse",
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Tags = Tags.Internal,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public Save_All_Payload_To_Fr8_Warehouse(ICrateManager crateManager, IContainer container)
            : base(crateManager)
        {
            _container = container;
        }

        public override Task Run()
        {
            using (var uow = _container.GetInstance<IUnitOfWork>())
            {
                var curCrates = Payload.CratesOfType<Manifest>().Where(c => c.ManifestType.Id != (int)MT.OperationalStatus);
                //get the process payload
                foreach (var curCrate in curCrates)
                {
                    var curManifest = curCrate.Content;
                    uow.MultiTenantObjectRepository.AddOrUpdate(CurrentUserId, curManifest);
                }

                uow.SaveChanges();
                Success();
            }

            return Task.FromResult(0);
        }

        public override async Task Initialize()
        {
            var textBlock = UiBuilder.GenerateTextBlock("", "This Action doesn't require any configuration.", "well well-lg");
            AddControl(textBlock);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}