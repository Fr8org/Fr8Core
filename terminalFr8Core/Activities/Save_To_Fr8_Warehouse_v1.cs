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

namespace terminalFr8Core.Activities
{
    public class Save_To_Fr8_Warehouse_v1 : ExplicitTerminalActivity
    {
        private readonly IContainer _container;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Save_To_Fr8_Warehouse",
            Label = "Save To Fr8 Warehouse",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public Save_To_Fr8_Warehouse_v1(ICrateManager crateManager, IContainer container)
            : base(crateManager)
        {
            _container = container;
        }

        public override Task Run()
        {
            // get the selected event from the drop down
            var crateChooser = GetControl<UpstreamCrateChooser>("UpstreamCrateChooser");
            using (var uow = _container.GetInstance<IUnitOfWork>())
            {
                var manifestTypes = crateChooser.SelectedCrates.Select(c => c.ManifestType.Value);
                var curCrates = Payload.CratesOfType<Manifest>().Where(c => manifestTypes.Contains(c.ManifestType.Id.ToString(CultureInfo.InvariantCulture)));
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
            var configControls = new StandardConfigurationControlsCM();
            configControls.Controls.Add(ControlHelper.CreateUpstreamCrateChooser("UpstreamCrateChooser", "Store which crates?"));
            var curConfigurationControlsCrate = PackControls(configControls);
            
            Storage.Clear();
            Storage.Add(curConfigurationControlsCrate);
           
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}