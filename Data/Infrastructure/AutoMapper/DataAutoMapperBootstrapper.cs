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

            Mapper.CreateMap<ActionDO, ActionDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.UserLabel, opts => opts.ResolveUsing(ad => ad.UserLabel))
                .ForMember(a => a.ActionType, opts => opts.ResolveUsing(ad => ad.ActionType))
                .ForMember(a => a.ActionListId, opts => opts.ResolveUsing(ad => ad.ActionListId))
                .ForMember(a => a.ConfigurationSettings, opts => opts.ResolveUsing(ad => ad.ConfigurationSettings))
                .ForMember(a => a.FieldMappingSettings, opts => opts.ResolveUsing(ad => ad.FieldMappingSettings))
                .ForMember(a => a.ParentPluginRegistration, opts => opts.ResolveUsing(ad => ad.ParentPluginRegistration));

            Mapper.CreateMap<ActionDTO, ActionDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
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

            Mapper.CreateMap<ActionListDO, ActionListDTO>();

            Mapper.CreateMap<ProcessTemplateDTO, ProcessTemplateDO>();
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
        }
    }
}