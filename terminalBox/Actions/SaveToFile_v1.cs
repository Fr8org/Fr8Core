using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using terminalBox.DataTransferObjects;
using terminalBox.Services;
using TerminalBase.BaseClasses;

namespace terminalBox.Actions
{
    public class SaveToFile_v1 : EnhancedTerminalActivity<SaveToFile_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser FileChooser { get; set; }
            public DropDownList BoxFolder { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                Controls.Add(FileChooser = uiBuilder.CreateCrateChooser("FileChooser", "Select data to store", true, true));
                Controls.Add(BoxFolder = new DropDownList()
                {
                    Label = "Select where to store file",
                    Name = "BoxFolder"
                });
            }
        }

        public SaveToFile_v1() 
            : base(true)
        {
        }

        private async Task FillAvailableFolders()
        {
            var token = JsonConvert.DeserializeObject<BoxAuthDTO>(AuthorizationToken.Token);
            var folders = await new BoxService().ListFolders(token);

            ConfigurationControls.BoxFolder.ListItems = folders.Select(x => new ListItem()
            {
                Key = x.Value,
                Value = x.Key
            }).ToList();
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
           await FillAvailableFolders();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            await FillAvailableFolders();
        }

        protected override Task RunCurrentActivity()
        {
            return Task.FromResult(0);
        }
    }
}