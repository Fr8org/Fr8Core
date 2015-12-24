using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;

namespace terminalFr8Core.Services
{
    public class DocuSignTemplateToStandardFileDescriptionConversion : ICrateConversion
    {
        public StandardFileHandleMS ConvertToStandardFileHandle(DocuSignTemplateCM input)
        {
            return null;
        }

        public Crate Convert(Crate input)
        {
            var inputManifest = input.Get() as DocuSignTemplateCM;
            if (inputManifest == null)
            {
                throw new Exception("Unknown crate type passed to DocuSignTemplateToStandardFileDescriptionConversion");
            }
            var outputManifest = ConvertToStandardFileHandle(inputManifest);
            return Crate.FromContent("From DocuSignTemplate To StandardFileDescription", outputManifest);
        }
    }
}