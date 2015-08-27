using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Utilities.AutoMapper;

namespace Data.Infrastructure.AutoMapper
{
    public class DataAutoMapperBootStrapper
    {
        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<string, JToken>().ConvertUsing<StringToJTokenConverter>();
            Mapper.CreateMap<JToken, string>().ConvertUsing<JTokenToStringConverter>();

            Mapper.CreateMap<ActionDO, ActionDesignDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.UserLabel, opts => opts.ResolveUsing(ad => ad.UserLabel))
                .ForMember(a => a.ActionType, opts => opts.ResolveUsing(ad => ad.ActionType))
                .ForMember(a => a.ActionListId, opts => opts.ResolveUsing(ad => ad.ActionListId))
                .ForMember(a => a.ConfigurationSettings, opts => opts.ResolveUsing(ad => ad.ConfigurationSettings))
                .ForMember(a => a.FieldMappingSettings, opts => opts.ResolveUsing(ad => ad.FieldMappingSettings))
                .ForMember(a => a.ParentPluginRegistration, opts => opts.ResolveUsing(ad => ad.ParentPluginRegistration));

            Mapper.CreateMap<ActionDesignDTO, ActionDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.UserLabel, opts => opts.ResolveUsing(ad => ad.UserLabel))
                .ForMember(a => a.ActionType, opts => opts.ResolveUsing(ad => ad.ActionType))
                .ForMember(a => a.ActionListId, opts => opts.ResolveUsing(ad => ad.ActionListId))
                .ForMember(a => a.ConfigurationSettings, opts => opts.ResolveUsing(ad => ad.ConfigurationSettings))
                .ForMember(a => a.FieldMappingSettings, opts => opts.ResolveUsing(ad => ad.FieldMappingSettings))
                .ForMember(a => a.ParentPluginRegistration, opts => opts.ResolveUsing(ad => ad.ParentPluginRegistration));

            Mapper.CreateMap<ActionRegistrationDO, ActionRegistrationDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.ActionType, opts => opts.ResolveUsing(x => x.ActionType))
                .ForMember(x => x.ParentPluginRegistration, opts => opts.ResolveUsing(x => x.ParentPluginRegistration))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version));

            Mapper.CreateMap<ActionRegistrationDTO, ActionRegistrationDO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.ActionType, opts => opts.ResolveUsing(x => x.ActionType))
                .ForMember(x => x.ParentPluginRegistration, opts => opts.ResolveUsing(x => x.ParentPluginRegistration))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version));

            Mapper.CreateMap<ActionDO, ActionPayloadDTO>()
                .ForMember(dest => dest.Id, opts => opts.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.UserLabel, opts => opts.ResolveUsing(src => src.UserLabel))
                .ForMember(dest => dest.ActionType, opts => opts.ResolveUsing(src => src.ActionType))
                .ForMember(dest => dest.ActionListId, opts => opts.ResolveUsing(src => src.ActionListId))
                .ForMember(dest => dest.ConfigurationSettings,
                    opts => opts.ResolveUsing(src => src.ConfigurationSettings))
                .ForMember(dest => dest.ParentPluginRegistration,
                    opts => opts.ResolveUsing(src => src.ParentPluginRegistration))
                .ForMember(dest => dest.PayloadMappings, opts => opts.ResolveUsing<PayloadMappingResolver>());
              

            Mapper.CreateMap<ActionPayloadDTO, ActionDO>();

            Mapper.CreateMap<ActionListDO, ActionListDTO>();

            Mapper.CreateMap<IList<DocuSignTemplateSubscriptionDO>, IList<string>>().ConvertUsing<DocuSignTemplateSubscriptionToStringConverter>();
            Mapper.CreateMap<IList<string>, IList<DocuSignTemplateSubscriptionDO>>().ConvertUsing<StringToDocuSignTemplateSubscriptionConverter>();
            Mapper.CreateMap<IList<ExternalEventSubscriptionDO>, IList<int>>().ConvertUsing<ExternalEventSubscriptionToIntConverter>();
            Mapper.CreateMap<IList<int>, IList<ExternalEventSubscriptionDO>>().ConvertUsing<IntToExternalEventSubscriptionConverter>();

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

            Mapper.CreateMap<DocuSign.Integrations.Client.Signer, Data.Wrappers.Signer>();

            Mapper.CreateMap<DocuSign.Integrations.Client.Account, Data.Wrappers.DocuSignAccount>();
            Mapper.CreateMap<DocuSign.Integrations.Client.TemplateInfo, DocuSignTemplateDTO>();
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