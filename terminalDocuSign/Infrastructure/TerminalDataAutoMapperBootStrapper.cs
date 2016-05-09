
namespace terminalDocuSign.Infrastructure.AutoMapper
{
    public class TerminalDataAutoMapperBootStrapper
    {
        public static void ConfigureAutoMapper()
        {
            //Mapper.CreateMap<JObject, DocuSignTemplateDTO>().ConvertUsing<JObjectToDocuSignTemplateDTOConverter>();
        }
    }
    /// <summary>
    /// AutoMapper converter to convert JObject representation to DocuSignTemplateDTO.
    /// </summary>
    //public class JObjectToDocuSignTemplateDTOConverter : ITypeConverter<JObject, DocuSignTemplateDTO>
    //{
    //	DocuSignTemplateDTO ITypeConverter<JObject, DocuSignTemplateDTO>.Convert(ResolutionContext context)
    //	{
    //		if (context.SourceValue == null)
    //			return null;
    //		var jObjTemplate = context.SourceValue as JObject;
    //		string templateId = jObjTemplate.SelectToken("envelopeTemplateDefinition.templateId").ToString();
    //		string name = jObjTemplate.SelectToken("envelopeTemplateDefinition.name").ToString();
    //		string description = jObjTemplate.SelectToken("envelopeTemplateDefinition.description").ToString();
    //		DocuSignTemplateDTO docuSignTemplateDTO = new DocuSignTemplateDTO()
    //		{
    //			Id = templateId,
    //			Name = name,
    //			Description = description,
    //			EnvelopeData = Mapper.Map<DocuSignEnvelopeDTO>(EnvelopeCreate.FromJson(jObjTemplate.ToString())),
    //		};
    //		return docuSignTemplateDTO;
    //	}
    //}
}