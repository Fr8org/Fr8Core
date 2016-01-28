using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using AutoMapper.Internal;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;
using StructureMap;
using Hub.Interfaces;
using System.IO;
using System.Text;

namespace terminalFr8Core.Actions
{
    public class GetFileFromFr8Store_v1 : BaseTerminalActivity
    {
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            var controlsMS = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

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
            var configurationControlsCrate = CreateControlsCrate();
            
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                updater.CrateStorage.Add(await GetCurrentUsersFiles());
            }

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            var payloadStorage = Crate.GetStorage(curPayloadDTO);

            var configContrls = GetConfigurationControls(curActivityDO);
            var fileSelector = configContrls.FindByName<DropDownList>("FileSelector");
            
            if (string.IsNullOrEmpty(fileSelector.Value))
            {
                return Error(curPayloadDTO, "No File was selected on design time", ActionErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            //let's download this file
            var file = await HubCommunicator.DownloadFile(int.Parse(fileSelector.Value), CurrentFr8UserId);

            if (file == null || file.Length < 1)
            {
                return Error(curPayloadDTO, "Unable to download file from Hub", ActionErrorCode.OPERATION_FAILED);
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

            var fileCrate = Data.Crates.Crate.FromContent("DownloadFile", fileDescription);

            using (var updater = Crate.UpdateStorage(curPayloadDTO))
            {
                updater.CrateStorage.Add(fileCrate);
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

        public async Task<Crate> GetCurrentUsersFiles()
        {
            var curAccountFileList = await HubCommunicator.GetFiles(CurrentFr8UserId);
            //TODO where tags == Docusign files
            var fileFields = curAccountFileList.Select(c => new FieldDTO(c.OriginalFileName, c.Id.ToString(CultureInfo.InvariantCulture)));
            return Crate.CreateDesignTimeFieldsCrate("AvailableFiles", fileFields.ToArray());
        }

        private Crate CreateControlsCrate()
        {
            var fileSelectionDropdown = new DropDownList
            {
                Label = "Please select file to get from Fr8 Store",
                Name = "FileSelector",
                Source = new FieldSourceDTO
                {
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    Label = "AvailableFiles"
                }
            };

            return PackControlsCrate(fileSelectionDropdown);
        }
    }
}