using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.States;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using Data.Entities;
using Newtonsoft.Json;

namespace terminalDocuSign.Services
{
    public class DocuSignManager
    {
        protected ICrateManager Crate;

        public DocuSignManager()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
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

        public Crate PackCrate_DocuSignTemplateNames(DocuSignAuth authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id, Availability = AvailabilityType.Configuration }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }


        public StandardPayloadDataCM CreateActionPayload(ActionDO curActionDO, AuthorizationTokenDO authTokenDO, string curEnvelopeId)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);

            var docusignEnvelope = new DocuSignEnvelope(
                docuSignAuthDTO.Email,
                docuSignAuthDTO.ApiPassword);

            var curEnvelopeData = docusignEnvelope.GetEnvelopeData(curEnvelopeId);
            var fields = GetDocuSignTemplateUserDefinedFields(curActionDO);

            if (fields == null || fields.Count == 0)
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDO.Id);
            }

            var payload = docusignEnvelope.ExtractPayload(fields, curEnvelopeId, curEnvelopeData);

            return new StandardPayloadDataCM(payload.ToArray());
        }

        public List<FieldDTO> GetDocuSignTemplateUserDefinedFields(ActionDO curActionDO)
        {
            var fieldsCrate = Crate.GetStorage(curActionDO).CratesOfType<StandardDesignTimeFieldsCM>().FirstOrDefault(x => x.Label == "DocuSignTemplateUserDefinedFields");

            if (fieldsCrate == null) return null;

            var manifestSchema = fieldsCrate.Content;

            if (manifestSchema == null
                || manifestSchema.Fields == null
                || manifestSchema.Fields.Count == 0)
            {
                return null;
            }

            return manifestSchema.Fields;
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
                var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);
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
        /// <param name="docuSignAuthDTO">DocuSign authentication token.</param>
        /// <param name="curActionDO">ActionDO object representing the current action. The crate with extracted 
        /// fields will be added to this Action replacing any older instances of that crate.</param>
        public IEnumerable<FieldDTO> ExtractFieldsAndAddToCrate(string docuSignTemplateId, DocuSignAuth docuSignAuthDTO, ActionDO curActionDO)
        {
            if (!string.IsNullOrEmpty(docuSignTemplateId))
            {
                var docusignEnvelope = new DocuSignEnvelope(
                    docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

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

        public Crate CrateCrateFromFields(string docuSignTemplateId, DocuSignAuth docuSignAuthDTO, string crateLabel)
        {
            if (!string.IsNullOrEmpty(docuSignTemplateId))
            {
                var docusignEnvelope = new DocuSignEnvelope(
                    docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

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
    }
}