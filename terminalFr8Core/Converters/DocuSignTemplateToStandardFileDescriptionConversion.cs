using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;

namespace terminalFr8Core.Converters
{
    public class DocuSignTemplateToStandardFileDescriptionConversion : ICrateConversion
    {
        public static readonly string ConversionLabel = "From DocuSignTemplate To StandardFileDescription";
        public StandardFileHandleMS ConvertToStandardFileHandle(DocuSignTemplateCM input)
        {
            return new StandardFileHandleMS
            {
                TextRepresentation = JsonConvert.SerializeObject(input)
            };
        }

        public Crate Convert(Crate input)
        {
            var inputManifest = input.Get() as DocuSignTemplateCM;
            if (inputManifest == null)
            {
                throw new Exception("Unknown crate type passed to DocuSignTemplateToStandardFileDescriptionConversion");
            }
            var outputManifest = ConvertToStandardFileHandle(inputManifest);
            return Crate.FromContent(ConversionLabel, outputManifest);
        }
    }
}