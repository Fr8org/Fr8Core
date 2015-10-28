using Core.Managers;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSign.Services
{
    public class DocuSignManager
    {
        protected ICrateManager Crate;

        public DocuSignManager()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        public DropDownListControlDefinitionDTO CreateDocuSignTemplatePicker(
            bool addOnChangeEvent, 
            string name = "Selected_DocuSign_Template", 
            string label = "Select DocuSign Template")
        {
            var control = new DropDownListControlDefinitionDTO()
            {
                Label = label,
                Name = name,
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
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

        public CrateDTO PackCrate_DocuSignTemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }
    }
}