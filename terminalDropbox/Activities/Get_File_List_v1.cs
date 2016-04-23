using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDropbox.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDropbox.Activities
{
    public class Get_File_List_v1 : BaseTerminalActivity
    {
        private readonly DropboxService _dropboxService;
        protected ICrateManager _crateManager;

        public Get_File_List_v1()
        {
            _dropboxService = ObjectFactory.GetInstance<DropboxService>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
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
                crateStorage.Add(PackCrate_DropboxFileList(fileNames));
            }

            return Success(payloadCrates);
        }

        private Crate PackCrate_DropboxFileList(List<string> fileNames)
        {
            return Data.Crates.Crate.FromJson("Dropbox File List", JsonConvert.SerializeObject(fileNames));
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return ConfigurationRequestType.Initial;
        }
    }
}