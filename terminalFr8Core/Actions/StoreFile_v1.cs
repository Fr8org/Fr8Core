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
using terminalFr8Core.Services;
using Utilities;
using StructureMap;
using Hub.Interfaces;
using System.IO;

namespace terminalFr8Core.Actions
{
    public class StoreFile_v1 : BaseTerminalAction
    {
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }


        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                await UpdateUpstreamFileCrates(curActionDO, updater.CrateStorage);
            }

            return curActionDO;
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);

            var designTimeStorage = Crate.GetStorage(curActionDO);
            var designTimeControls = designTimeStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var textSourceControl = designTimeControls.Controls.Single(c => c.Name == "File Crate label" && c.Type == ControlTypes.TextSource);

            if (string.IsNullOrEmpty(textSourceControl.Value))
            {
                return Error(curPayloadDTO, "No Label was selected on design time", ActionErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            var payloadStorage = Crate.GetStorage(curPayloadDTO);
            //we should upload this file to our file storage
            var userSelectedFileManifest = payloadStorage.CrateContentsOfType<StandardFileDescriptionCM>(f => f.Label == textSourceControl.Value).FirstOrDefault();
            if (userSelectedFileManifest == null)
            {
                return Error(curPayloadDTO, "No StandardFileDescriptionCM Crate was found with label "+textSourceControl.Value, ActionErrorCode.PAYLOAD_DATA_MISSING);
            }


            var fileContents = userSelectedFileManifest.TextRepresentation;

            using (var stream = GenerateStreamFromString(fileContents))
            {
                //TODO what to do with this fileDO??
                var fileDO = await HubCommunicator.SaveFile("testFile", stream);
            }

            
            return Success(curPayloadDTO);
        }

        private MemoryStream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public async Task UpdateUpstreamFileCrates(ActionDO curActionDO, CrateStorage storage)
        {
            // Build a crate with the list of available upstream fields
            var curUpstreamFieldsCrate = storage.SingleOrDefault(c => c.ManifestType.Id == (int)MT.StandardDesignTimeFields
                                                                                && c.Label == "Upstream Terminal-Provided File Crates");
            if (curUpstreamFieldsCrate != null)
            {
                storage.Remove(curUpstreamFieldsCrate);
            }

            var upstreamFileCrates = await GetCratesByDirection<StandardFileDescriptionCM>(curActionDO, CrateDirection.Upstream);

            var curUpstreamFields = upstreamFileCrates.Select(c => new FieldDTO(c.Label, c.Label)).ToArray();

            curUpstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided File Crates", curUpstreamFields);
            storage.Add(curUpstreamFieldsCrate);
            
        }

        private Crate CreateControlsCrate()
        {
            var textSource = new TextSource("File Crate Label", "Upstream Terminal-Provided File Crates", "File Crate label");
            /*var textSource = new TextSource
            {
                Label = "Store data from which upstream Crate of Files having Label:",
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                Source = new FieldSourceDTO
                {
                    Label = "Available File Manifest Labels",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                },
                Value = null,
                UpstreamSourceLabel = null
            };*/

            return PackControlsCrate(textSource);
        }
    }
}