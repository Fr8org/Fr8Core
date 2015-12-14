using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Constants;
using Data.Control;
using Data.Crates;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using Data.Entities;

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

        public Crate PackCrate_DocuSignTemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }

        /// <summary>
        /// Extracts fields from a DocuSign envelope.
        /// </summary>
        /// <param name="docuSignTemplateId">DocuSign TemplateId.</param>
        /// <param name="docuSignAuthDTO">DocuSign authentication token.</param>
        /// <param name="curActionDO">ActionDO object representing the current action. The crate with extracted 
        /// fields will be added to this Action replacing any older instances of that crate.</param>
        public void ExtractFieldsAndAddToCrate(string docuSignTemplateId, DocuSignAuthDTO docuSignAuthDTO, ActionDO curActionDO)

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

                using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))

                {
                    updater.CrateStorage.RemoveByManifestId((int) MT.StandardDesignTimeFields);
                    updater.CrateStorage.Add(Crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", fieldCollection.ToArray()));
                }
            }
        }

        public Crate CrateCrateFromFields(string docuSignTemplateId, DocuSignAuthDTO docuSignAuthDTO, string crateLabel)
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