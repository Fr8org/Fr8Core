using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using terminalDropbox.Interfaces;
using System.IO;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using System;

namespace terminalDropbox.Actions
{
    public class Get_File_List_v1 : TerminalActivity<Get_File_List_v1.ActivityUi>
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

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("1cbc96d3-7d61-4acc-8cf0-6a3f0987b00d"),
            Version = "1",
            Name = "Get_File_List",
            Label = "Get File List",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
      

        public Get_File_List_v1(ICrateManager crateManager, IDropboxService dropboxService)
            : base(crateManager)
        {
            _dropboxService = dropboxService;
        }

        public override async Task Initialize()
        {
            var fileNames = await _dropboxService.GetFileList(AuthorizationToken);
            ActivityUI.FileList.ListItems = fileNames
                .Select(filePath => new ListItem { Key = Path.GetFileName(filePath), Value = Path.GetFileName(filePath) }).ToList();

            CrateSignaller.MarkAvailableAtRuntime<StandardFileListCM>(RuntimeCrateLabel);

            Storage.ReplaceByLabel(PackDropboxFileListCrate(fileNames));
        }

        public override async Task FollowUp()
        {
            var fileList = await _dropboxService.GetFileList(AuthorizationToken);
            ActivityUI.FileList.ListItems = fileList
                .Select(filePath => new ListItem { Key = Path.GetFileName(filePath), Value = Path.GetFileName(filePath) }).ToList();
            var file = await _dropboxService.GetFile(AuthorizationToken,"/" + ActivityUI.FileList.ListItems.Where(s => s.Key == ActivityUI.FileList.selectedKey).FirstOrDefault().Key);

            CrateSignaller.MarkAvailableAtRuntime<StandardFileDescriptionCM>("Dropbox selected file");
            Storage.ReplaceByLabel(Crate<StandardFileDescriptionCM>.FromContent("Dropbox selected file", file));
            

            CrateSignaller.MarkAvailableAtRuntime<StandardFileListCM>(RuntimeCrateLabel).AddFields(fileList.Select(f=>Path.GetFileName(f)).ToArray());
            Storage.ReplaceByLabel(PackDropboxFileListCrate(fileList));
        }

      

        public override async Task Run()
        {
            IList<string> fileNames;
            StandardFileDescriptionCM file;
            var selectedFileName = ActivityUI.Controls.Where(s => s.Label == "Select a file").FirstOrDefault().Value;
            try
            {
                fileNames = await _dropboxService.GetFileList(AuthorizationToken);
                if (!string.IsNullOrEmpty(selectedFileName))
                {
                    file = await _dropboxService.GetFile(AuthorizationToken, "/" + selectedFileName);
                    Payload.Add(Crate<StandardFileDescriptionCM>.FromContent("Dropbox selected file", file));
                }
            }
            catch (Dropbox.Api.AuthException)
            {
                throw new AuthorizationTokenExpiredOrInvalidException();
            }
            var dropboxFileList = PackDropboxFileListCrate(fileNames);
            Payload.Add(dropboxFileList);
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
                var fileSharedUrl = _dropboxService.GetFileSharedUrl(AuthorizationToken, filePath);

                fileDesc.DirectUrl = fileSharedUrl;
                descriptionList.Add(fileDesc);
            }

            return Crate<StandardFileListCM>.FromContent(
                RuntimeCrateLabel,
                new StandardFileListCM
                {
                    FileList = descriptionList
                });

        }
    }
}