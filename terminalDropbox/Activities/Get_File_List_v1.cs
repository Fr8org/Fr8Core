using Data.Entities;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using terminalDropbox.Interfaces;
using TerminalBase.BaseClasses;
using terminalDropbox.Services;
using System.IO;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Errors;

namespace terminalDropbox.Actions
{
    public class Get_File_List_v1 : EnhancedTerminalActivity<Get_File_List_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList FileList { get; set; }

            public ActivityUi()
        {
                FileList = new DropDownList
            {
                    Label = "Select a file",
                    Name = nameof(FileList),
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls.Add(FileList);
            }
        }

        private const string RuntimeCrateLabel = "Dropbox file list";

        private readonly IDropboxService _dropboxService;

        public Get_File_List_v1() : base(true)
        {
            _dropboxService = ObjectFactory.GetInstance<DropboxService>();
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var fileNames = await _dropboxService.GetFileList(GetDropboxAuthToken());
            ConfigurationControls.FileList.ListItems = fileNames
                .Select(filePath => new ListItem { Key = Path.GetFileName(filePath), Value = Path.GetFileName(filePath) }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardFileListCM>(RuntimeCrateLabel);
            CurrentActivityStorage.ReplaceByLabel(PackDropboxFileListCrate(fileNames));
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
            {
            var fileList = await _dropboxService.GetFileList(GetDropboxAuthToken());
            ConfigurationControls.FileList.ListItems = fileList
                .Select(filePath => new ListItem { Key = Path.GetFileName(filePath), Value = Path.GetFileName(filePath) }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardFileListCM>(RuntimeCrateLabel);
            CurrentActivityStorage.ReplaceByLabel(PackDropboxFileListCrate(fileList));
            }

        private AuthorizationTokenDO GetDropboxAuthToken(AuthorizationTokenDO authTokenDO = null)
            {
            return authTokenDO ?? AuthorizationToken;
            }

        protected override async Task RunCurrentActivity()
        {
            IList<string> fileNames;
            try
            {
                fileNames = await _dropboxService.GetFileList(GetDropboxAuthToken());
            }
            catch (Dropbox.Api.AuthException)
            {
                throw new AuthorizationTokenExpiredOrInvalidException();
            }
            var dropboxFileList = PackDropboxFileListCrate(fileNames);
            CurrentPayloadStorage.Add(dropboxFileList);
        }

        private Crate<StandardFileListCM> PackDropboxFileListCrate(IEnumerable<string> fileList)
        {
            var descriptionList = new List<StandardFileDescriptionCM>();
            foreach (var filePath in fileList)
            {
                var fileDesc = new StandardFileDescriptionCM()
        {
                    Filename = Path.GetFileName(filePath),
                    Filetype = Path.GetExtension(filePath)
                };
                var fileSharedUrl = _dropboxService.GetFileSharedUrl(GetDropboxAuthToken(), filePath);

                fileDesc.DirectUrl = fileSharedUrl;
                descriptionList.Add(fileDesc);
        }

            return Crate<StandardFileListCM>.FromContent(
                RuntimeCrateLabel,
                new StandardFileListCM
        {
                    FileList = descriptionList
                },
                AvailabilityType.Always);

        }
    }
}