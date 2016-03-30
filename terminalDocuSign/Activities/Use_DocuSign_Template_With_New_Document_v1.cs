using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;
using TerminalBase.Infrastructure.Behaviors;
using terminalDocuSign.Services.New_Api;
using terminalDocuSign.Actions;

namespace terminalDocuSign.Actions
{
    public class Use_DocuSign_Template_With_New_Document_v1 : Send_DocuSign_Envelope_v1
    {
        protected override string ActivityUserFriendlyName => "Use DocuSign Template With New Document";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            var loginInfo = DocuSignManager.SetUp(authTokenDO);

            var filehandler = CrateManager.GetByManifest<StandardFileDescriptionCM>(payloadCrates);

            if (filehandler == null || string.IsNullOrEmpty(filehandler.TextRepresentation))
                return Error(payloadCrates, "No file handle was found", ActivityErrorCode.PAYLOAD_DATA_MISSING);

            return HandleTemplateData(curActivityDO, loginInfo, payloadCrates, filehandler);
        }

        private PayloadDTO HandleTemplateData(ActivityDO curActivityDO, DocuSignApiConfiguration loginInfo, PayloadDTO payloadCrates, StandardFileDescriptionCM filehandler)
        {
            var curTemplateId = ExtractTemplateId(curActivityDO);
            var payloadCrateStorage = CrateManager.GetStorage(payloadCrates);
            var fieldList = MapControlsToFields(CrateManager.GetStorage(curActivityDO), payloadCrateStorage);
            var rolesList = MapRoleControlsToFields(CrateManager.GetStorage(curActivityDO), payloadCrateStorage);
            try
            {
                DocuSignManager.SendAnEnvelopeFromTemplate(loginInfo, rolesList, fieldList, curTemplateId, filehandler);
            }
            catch (Exception ex)
            {
                return Error(payloadCrates, $"Couldn't send an envelope. {ex}");
            }
            return Success(payloadCrates);
        }

        protected override Crate CreateDocusignTemplateConfigurationControls()
        {
            var infoBox = new TextBlock() { Value = @"This Activity overlays the tabs from an existing Template onto a new Document and sends out a DocuSign Envelope. 
                                                        When this Activity executes, it will look for and expect to be provided from upstream with one Excel or Word file." };

            var fieldSelectDocusignTemplateDTO = new DropDownList
            {
                Label = "Use DocuSign Template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<ControlEvent>()
                {
                     ControlEvent.RequestConfig
                },
                Source = null
            };

            var fieldsDTO = new List<ControlDefinitionDTO>
            {
                infoBox, fieldSelectDocusignTemplateDTO
            };

            var controls = new StandardConfigurationControlsCM
            {
                Controls = fieldsDTO
            };

            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }
    }
}