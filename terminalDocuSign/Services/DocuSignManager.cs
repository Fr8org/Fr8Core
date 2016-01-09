using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.States;
using DocuSign.Integrations.Client;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using Data.Entities;
using Newtonsoft.Json;
using terminalDocuSign.Actions;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Interfaces;

namespace terminalDocuSign.Services
{
    public class DocusignQuery
    {
        public static readonly FieldDTO[] Statuses = 
        {
            new FieldDTO("Any status", "<any>"),
            new FieldDTO("Sent", "sent"),
            new FieldDTO("Delivered", "delivered"),
            new FieldDTO("Signed", "signed"),
            new FieldDTO("Completed", "completed"),
            new FieldDTO("Declined", "declined"),
            new FieldDTO("Voided", "voided"),
            new FieldDTO("Timed Out", "timedout"),
            new FieldDTO("Authoritative Copy", "authoritativecopy"),
            new FieldDTO("Transfer Completed", "transfercompleted"),
            new FieldDTO("Template", "template"),
            new FieldDTO("Correct", "correct"),
            new FieldDTO("Created", "created"),
            new FieldDTO("Delivered", "delivered"),
            new FieldDTO("Signed", "signed"),
            new FieldDTO("Declined", "declined"),
            new FieldDTO("Completed", "completed"),
            new FieldDTO("Fax Pending", "faxpending"),
            new FieldDTO("Auto Responded", "autoresponded"),
        };

        public string SearchText;
        public DateTime? FromDate;
        public DateTime? ToDate;
        public string Status;
        public string Folder;
    }

    public class DocuSignManager
    {
        protected ICrateManager Crate;
        private readonly IDocuSignFolder _docuSignFolder;

        public DocuSignManager()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
            _docuSignFolder = ObjectFactory.GetInstance<IDocuSignFolder>();
        }

        public static DropDownList CreateDocuSignTemplatePicker(
            bool addOnChangeEvent,
            string name = "Selected_DocuSign_Template",
            string label = "Select DocuSign Template")
        {
            var control = new DropDownList()
            {
                Label = label,
                Name = name,
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            if (addOnChangeEvent)
            {
                control.Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                };
            }

            return control;
        }

        public DocuSignTemplateDTO DownloadDocuSignTemplate(DocuSignAuthTokenDTO authToken, string templateId)
        {
            var template = new DocuSignTemplate();
            var templateDTO = template.GetTemplateById(authToken.Email, authToken.ApiPassword, templateId);
            return templateDTO;
        }

        public Crate PackCrate_DocuSignTemplateNames(DocuSignAuthTokenDTO authToken)
        {
            var template = new DocuSignTemplate();
            var templates = template.GetTemplates(authToken.Email, authToken.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() {Key = x.Name, Value = x.Id, Availability = AvailabilityType.Configuration}).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }


        public StandardPayloadDataCM CreateActionPayload(ActionDO curActionDO, AuthorizationTokenDO authTokenDO, string curEnvelopeId)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            var docusignEnvelope = new DocuSignEnvelope(
                docuSignAuthDTO.Email,
                docuSignAuthDTO.ApiPassword);

            var curEnvelopeData = docusignEnvelope.GetEnvelopeData(curEnvelopeId);
            var templateFields = ExtractFieldsAndAddToCrate(curEnvelopeId, docuSignAuthDTO, curActionDO);
            var payload = docusignEnvelope.ExtractPayload(templateFields.ToList(), curEnvelopeId, curEnvelopeData);

            return new StandardPayloadDataCM(payload.ToArray());
        }


        //Has to be retrofit after https://maginot.atlassian.net/browse/FR-1280 is done
        public string GetEnvelopeIdFromPayload(PayloadDTO curPayloadDTO)
        {
            var standardPayload = Crate.FromDto(curPayloadDTO.CrateStorage).CrateContentsOfType<StandardPayloadDataCM>().FirstOrDefault();

            if (standardPayload == null)
            {
                return null;
            }

            var envelopeId = standardPayload.GetValues("EnvelopeId").FirstOrDefault();

            return envelopeId;
        }

        public int UpdateUserDefinedFields(ActionDO curActionDO, AuthorizationTokenDO authTokenDO, ICrateStorageUpdater updater, string envelopeId)
        {
            int fieldCount = 0;
            updater.CrateStorage.RemoveByLabel("DocuSignTemplateUserDefinedFields");
            if (!String.IsNullOrEmpty(envelopeId))
            {
                var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
                var userDefinedFields = this.ExtractFieldsAndAddToCrate(envelopeId, docuSignAuthDTO, curActionDO);
                updater.CrateStorage.Add(Crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", userDefinedFields.ToArray()));
                fieldCount = userDefinedFields.Count();
            }
            return fieldCount;
        }

        /// <summary>
        /// Extracts fields from a DocuSign envelope.
        /// </summary>
        /// <param name="docuSignTemplateId">DocuSign TemplateId.</param>
        /// <param name="docuSignAuthToken">DocuSign authentication token.</param>
        /// <param name="curActionDO">ActionDO object representing the current action. The crate with extracted 
        /// fields will be added to this Action replacing any older instances of that crate.</param>
        public IEnumerable<FieldDTO> ExtractFieldsAndAddToCrate(string docuSignTemplateId, DocuSignAuthTokenDTO docuSignAuthToken, ActionDO curActionDO)
        {
            if (!string.IsNullOrEmpty(docuSignTemplateId))
            {
                var docusignEnvelope = new DocuSignEnvelope(
                    docuSignAuthToken.Email, docuSignAuthToken.ApiPassword);

                var userDefinedFields = docusignEnvelope
                    .GetEnvelopeDataByTemplate(docuSignTemplateId);

                var fieldCollection = userDefinedFields
                    .Select(f => new FieldDTO
                    {
                        Key = f.Name,
                        Value = f.Value,
                        Availability = AvailabilityType.Configuration
                    });
                return fieldCollection;
            }

            throw new ApplicationException("Docusign TemplateId is null or emty");
        }

        public Crate CrateCrateFromFields(string docuSignTemplateId, DocuSignAuthTokenDTO docuSignAuthToken, string crateLabel)
        {
            if (!string.IsNullOrEmpty(docuSignTemplateId))
            {
                var docusignEnvelope = new DocuSignEnvelope(
                    docuSignAuthToken.Email, docuSignAuthToken.ApiPassword);

                var userDefinedFields = docusignEnvelope
                    .GetEnvelopeDataByTemplate(docuSignTemplateId);

                var fieldCollection = userDefinedFields
                    .Select(f => new FieldDTO
                    {
                        Key = f.Name,
                        Value = f.Value
                    });

                return Crate.CreateDesignTimeFieldsCrate(crateLabel, fieldCollection.ToArray());
            }
            return null;
        }
        
        public List<FolderItem> SearchDocusign(DocuSignAuthTokenDTO authToken, DocusignQuery query)
        {
            var envelopes = new List<FolderItem>();

            if (string.IsNullOrWhiteSpace(query.Folder) || query.Folder == "<any>")
            {
                foreach (var folder in _docuSignFolder.GetFolders(authToken.Email, authToken.ApiPassword))
                {
                    SearchFolder(authToken, query, folder.FolderId, envelopes);
                }
            }
            else
            {
                SearchFolder(authToken, query,  query.Folder, envelopes);
            }

            return envelopes;
        }

        private void SearchFolder(DocuSignAuthTokenDTO authToken, DocusignQuery query, string folder, List<FolderItem> envelopes)
        {
            envelopes.AddRange(_docuSignFolder.Search(authToken.Email, authToken.ApiPassword, query.SearchText, folder, query.Status == "<any>" ? null : query.Status, query.FromDate, query.ToDate));
        }
    }
}