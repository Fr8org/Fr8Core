using System;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Newtonsoft.Json;

namespace terminalFr8Core.Converters
{
    public class StandardFileDescriptionToDocuSignTemplateConversion : ICrateConversion
    {
        public static readonly string ConversionLabel = "From StandardFileDescription To DocuSignTemplate";
        public DocuSignTemplateCM ConvertToDocuSignTemplate(StandardFileDescriptionCM input)
        {
            return JsonConvert.DeserializeObject<DocuSignTemplateCM>(input.TextRepresentation);
        }

        public Crate Convert(Crate input)
        {
            var inputManifest = input.Get() as StandardFileDescriptionCM;
            if (inputManifest == null)
            {
                throw new Exception("Unknown crate type passed to StandardFileDescriptionToDocuSignTemplateConversion");
            }
            var outputManifest = ConvertToDocuSignTemplate(inputManifest);
            return Crate.FromContent(ConversionLabel, outputManifest);
        }
    }
}