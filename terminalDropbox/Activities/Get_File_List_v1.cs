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
using terminalDropbox.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDropbox.Services;

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

        private const string RunTimeCrateLabel = "Files from Dropbox";

        private readonly IDropboxService _dropboxService;
        private readonly ICrateManager _crateManager;

        public Get_File_List_v1() : base(true)
        {
            _dropboxService = ObjectFactory.GetInstance<IDropboxService>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            var fileNames = await _dropboxService.GetFileList(GetDropboxAuthToken());
            ConfigurationControls.FileList.ListItems = fileNames.Select(x => new ListItem { Key = x, Value = x }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardFileListCM>(RunTimeCrateLabel);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var fileNames = await _dropboxService.GetFileList(authTokenDO);

            using (var crateStorage = _crateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.AddRange(PackCrate_DropboxFileList(fileNames));
            }

            return Success(payloadCrates);
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var fileNames = await _dropboxService.GetFileList(GetDropboxAuthToken());
            ConfigurationControls.FileList.ListItems = fileNames.Select(x => new ListItem { Key = x, Value = x }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardFileListCM>(RunTimeCrateLabel);
        }

        private AuthorizationTokenDO GetDropboxAuthToken(AuthorizationTokenDO authTokenDO = null)
        {
            return authTokenDO ?? AuthorizationToken;
        }

        protected override Task RunCurrentActivity()
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Crate> PackCrate_DropboxFileList(List<string> fileNames)
        {
            return fileNames.Select(fileName => Crate.FromContent("File", new StandardFileDescriptionCM()
            {
                Filename = fileName
            }));
        }

        //public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        //{
        //    return ConfigurationRequestType.Initial;
        //}

        //protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        //{
        //    var fileNames = await _dropboxService.GetFileList(authTokenDO);

        //    using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
        //    {
        //        crateStorage.Clear();
        //        crateStorage.Add(PackCrate_DropboxFileList(fileNames));
        //    }

        //    return await Task.FromResult<ActivityDO>(curActivityDO);
        //}
    }
}