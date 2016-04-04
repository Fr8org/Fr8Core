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

        protected override PayloadDTO SendAnEnvelope(DocuSignApiConfiguration loginInfo, PayloadDTO payloadCrates,
            List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            try
            {

                DocuSignManager.SendAnEnvelopeFromTemplate(loginInfo, rolesList, fieldList, curTemplateId, null);
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

            var documentOverrideDDLB = new DropDownList
            {
                Label = "Use new document",
                Name = "document_Override_DDLB",
                Required = true,
                Source = new FieldSourceDTO()
                {
                    ManifestType = CrateManifestTypes.StandardFileDescription,
                    RequestUpstream = true,
                    AvailabilityType = AvailabilityType.RunTime,
                    Label = "File uploaded by Load Excel",
                }
            };

            var fieldsDTO = new List<ControlDefinitionDTO>
            {
                infoBox, fieldSelectDocusignTemplateDTO,documentOverrideDDLB
            };

            var controls = new StandardConfigurationControlsCM
            {
                Controls = fieldsDTO
            };

            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }
    }
}