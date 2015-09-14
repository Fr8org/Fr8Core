using System.Collections.Generic;
using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Wrappers;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities.AutoMapper;
using Signer = DocuSign.Integrations.Client.Signer;

namespace Data.Infrastructure.AutoMapper
{
    public class DataAutoMapperBootStrapper
    {
        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<string, JToken>().ConvertUsing<StringToJTokenConverter>();
            Mapper.CreateMap<JToken, string>().ConvertUsing<JTokenToStringConverter>();

            Mapper.CreateMap<ActionDO, ActionDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.ActionListId, opts => opts.ResolveUsing(ad => ad.ParentActivityId))
                 .ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.DeserializeObject<CrateStorageDTO>(ad.CrateStorage)))
                .ForMember(a => a.FieldMappingSettings, opts => opts.ResolveUsing(ad => ad.FieldMappingSettings))
                .ForMember(a => a.ActionTemplateId, opts => opts.ResolveUsing(ad => ad.ActionTemplateId))
                .ForMember(a => a.ActionTemplate, opts => opts.ResolveUsing(ad => ad.ActionTemplate));

            Mapper.CreateMap<ActionDTO, ActionDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.ParentActivityId, opts => opts.ResolveUsing(ad => ad.ActionListId))
                .ForMember(a => a.ActionTemplateId, opts => opts.ResolveUsing(ad => ad.ActionTemplateId))
                .ForMember(a => a.ActionTemplate, opts => opts.ResolveUsing(ad => ad.ActionTemplate))
                .ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.SerializeObject(ad.CrateStorage)))
                .ForMember(a => a.FieldMappingSettings, opts => opts.ResolveUsing(ad => ad.FieldMappingSettings))
                .ForMember(a => a.IsTempId, opts => opts.ResolveUsing(ad => ad.IsTempId));

            Mapper.CreateMap<ActionTemplateDO, ActionTemplateDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.DefaultEndPoint, opts => opts.ResolveUsing(x => x.DefaultEndPoint))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version));

            Mapper.CreateMap<ActionTemplateDTO, ActionTemplateDO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.DefaultEndPoint, opts => opts.ResolveUsing(x => x.DefaultEndPoint))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version));

            Mapper.CreateMap<ActionListDO, ActionListDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.ActionListType, opts => opts.ResolveUsing(x => x.ActionListType))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name));

            Mapper.CreateMap<IList<DocuSignTemplateSubscriptionDO>, IList<string>>().ConvertUsing<DocuSignTemplateSubscriptionToStringConverter>();
            Mapper.CreateMap<IList<string>, IList<DocuSignTemplateSubscriptionDO>>().ConvertUsing<StringToDocuSignTemplateSubscriptionConverter>();
            Mapper.CreateMap<IList<ExternalEventSubscriptionDO>, IList<int?>>().ConvertUsing<ExternalEventSubscriptionToIntConverter>();
            Mapper.CreateMap<IList<int?>, IList<ExternalEventSubscriptionDO>>().ConvertUsing<IntToExternalEventSubscriptionConverter>();

            Mapper.CreateMap<ProcessTemplateDO, ProcessTemplateDTO>();

            Mapper.CreateMap<ProcessNodeTemplateDTO, ProcessNodeTemplateDO>()
                .ForMember(x => x.ParentTemplateId, opts => opts.ResolveUsing(x => x.ProcessTemplateId));
            Mapper.CreateMap<ProcessNodeTemplateDO, ProcessNodeTemplateDTO>()
                .ForMember(x => x.ProcessTemplateId, opts => opts.ResolveUsing(x => x.ParentTemplateId));

            Mapper.CreateMap<CriteriaDO, CriteriaDTO>()
                .ForMember(x => x.Conditions, opts => opts.ResolveUsing(y => y.ConditionsJSON));
            Mapper.CreateMap<CriteriaDTO, CriteriaDO>()
                .ForMember(x => x.ConditionsJSON, opts => opts.ResolveUsing(y => y.Conditions));

            Mapper.CreateMap<ProcessTemplateDO, FullProcessTemplateDTO>()
                .ConvertUsing<ProcessTemplateDOFullConverter>();

            Mapper.CreateMap<Signer, Wrappers.Signer>();

            Mapper.CreateMap<Account, DocuSignAccount>();
            Mapper.CreateMap<TemplateInfo, DocuSignTemplateDTO>();
            Mapper.CreateMap<FileDO, FileDescriptionDTO>();

            Mapper.CreateMap<CrateStorageDTO, string>()
                .ConvertUsing<JSONToStringConverter<CrateStorageDTO>>();
            Mapper.CreateMap<string, CrateStorageDTO>()
                .ConvertUsing<StringToJSONConverter<CrateStorageDTO>>();

            Mapper.CreateMap<FieldMappingSettingsDTO, string>()
                .ConvertUsing<JSONToStringConverter<FieldMappingSettingsDTO>>();
            Mapper.CreateMap<string, FieldMappingSettingsDTO>()
                .ConvertUsing<StringToJSONConverter<FieldMappingSettingsDTO>>();
        }
    }

    internal class PayloadMappingResolver : ValueResolver<ActionDO, PayloadMappingsDTO>
    {
        protected override PayloadMappingsDTO ResolveCore(ActionDO source)
        {
            if (string.IsNullOrEmpty(source.PayloadMappings))
                return null;

            var payloadMappingDto = new PayloadMappingsDTO();
            payloadMappingDto.Deserialize(source.PayloadMappings);
            return payloadMappingDto;
        }
    }
}