using System;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Newtonsoft.Json;

namespace terminalFr8Core.Converters
{
    public class DocuSignTemplateToStandardFileDescriptionConversion : ICrateConversion
    {
        public static readonly string ConversionLabel = "From DocuSignTemplate To StandardFileDescription";
        public StandardFileDescriptionCM ConvertToStandardFileHandle(DocuSignTemplateCM input)
        {
            return new StandardFileDescriptionCM
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