using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using System.IO;
using System.Text;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

namespace terminalFr8Core.Actions
{
    public class GetFileFromFr8Store_v1 : BaseTerminalActivity
    {
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }


        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationCrate = CreateControlsCrate();
            await FillFileSelectorSource(configurationCrate, "FileSelector");

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationCrate));
            }

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            var payloadStorage = CrateManager.GetStorage(curPayloadDTO);

            var configContrls = GetConfigurationControls(curActivityDO);
            var fileSelector = configContrls.FindByName<DropDownList>("FileSelector");

            if (string.IsNullOrEmpty(fileSelector.Value))
            {
                return Error(curPayloadDTO, "No File was selected on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            //let's download this file
            var file = await HubCommunicator.DownloadFile(int.Parse(fileSelector.Value), CurrentFr8UserId);

            if (file == null || file.Length < 1)
            {
                return Error(curPayloadDTO, "Unable to download file from Hub");
            }

            string textRepresentation = string.Empty;
            using (var reader = new StreamReader(file, Encoding.UTF8))
            {
                textRepresentation = reader.ReadToEnd();
            }

            //let's convert this file to a string and store it in a file crate
            var fileDescription = new StandardFileDescriptionCM
            {
                Filename = fileSelector.selectedKey,
                TextRepresentation = textRepresentation
            };

            var fileCrate = Fr8Data.Crates.Crate.FromContent("DownloadFile", fileDescription);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curPayloadDTO))
            {
                crateStorage.Add(fileCrate);
            }

            return Success(curPayloadDTO);
        }

        private MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private Crate CreateControlsCrate()
        {
            var fileSelectionDropdown = new DropDownList
            {
                Label = "Please select file to get from Fr8 Store",
                Name = "FileSelector",
                Source = null
            };

            return PackControlsCrate(fileSelectionDropdown);
        }

        #region Fill Source
        private async Task FillFileSelectorSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = await GetCurrentUsersFiles();
            }
        }

        private async Task<List<ListItem>> GetCurrentUsersFiles()
        {
            var curAccountFileList = await HubCommunicator.GetFiles(CurrentFr8UserId);
            //TODO where tags == Docusign files
            return curAccountFileList.Select(c => new ListItem() { Key = c.OriginalFileName, Value = c.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
        }
        #endregion
    }
}