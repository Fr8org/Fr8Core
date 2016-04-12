using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Interfaces.Manifests;
using Data.States;
using terminalDropbox.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDropbox.Services;
using System.IO;

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
        private const string FileCrateLabel = "File from Dropbox";

        private readonly IDropboxService _dropboxService;

        public Get_File_List_v1() : base(true)
        {
            _dropboxService = ObjectFactory.GetInstance<DropboxService>();
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var fileNames = await _dropboxService.GetFileList(GetDropboxAuthToken());
            ConfigurationControls.FileList.ListItems = fileNames
                .Select(x => new ListItem { Key = x, Value = x }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardFileListCM>(RuntimeCrateLabel);
            CurrentActivityStorage.ReplaceByLabel(PackDropboxFileListCrate(fileNames.ToArray()));
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            var fileNames = await _dropboxService.GetFileList(GetDropboxAuthToken());
            ConfigurationControls.FileList.ListItems = fileNames
                .Select(x => new ListItem { Key = x, Value = x }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardFileListCM>(RuntimeCrateLabel);
            CurrentActivityStorage.ReplaceByLabel(PackDropboxFileListCrate(fileNames.ToArray()));
        }

        private AuthorizationTokenDO GetDropboxAuthToken(AuthorizationTokenDO authTokenDO = null)
        {
            return authTokenDO ?? AuthorizationToken;
        }

        protected override async Task RunCurrentActivity()
        {
            var mappedFields = new StandardPayloadDataCM(
                new FieldDTO("FileName", "[Files].[FileName]"),
                new FieldDTO("FileExtension", "[Files].[FileExtension]")
                );
            var values = new StandardPayloadDataCM(
                new FieldDTO("FileName", Path.GetFileName(ConfigurationControls.FileList.Value)),
                new FieldDTO("FileExtension", Path.GetExtension(ConfigurationControls.FileList.Value))
                );
            CurrentPayloadStorage.Add(Crate.FromContent("MappedFields", mappedFields, AvailabilityType.Always));
            CurrentPayloadStorage.Add(Crate.FromContent("TableData", values, AvailabilityType.Always));
        }

        private Crate<StandardFileListCM> PackDropboxFileListCrate(string[] fileNames)
        {
            List<StandardFileDescriptionCM> descriptionList = fileNames
                .Select(fileName => new StandardFileDescriptionCM()
                {
                    Filename = fileName
                })
                .ToList();

            return Crate<StandardFileListCM>.FromContent(
                RuntimeCrateLabel,
                new StandardFileListCM
                {
                    FileList = descriptionList
                });
        }
    }
}