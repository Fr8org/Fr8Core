using System;
using System.Collections.Generic;
using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
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
                .ForMember(a => a.ActivityTemplateId, opts => opts.ResolveUsing(ad => ad.ActivityTemplateId))
                .ForMember(a => a.CurrentView, opts => opts.ResolveUsing(ad => ad.currentView))
                // commented out by yakov.gnusin, this breaks design-time process.
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(ad => ad.ActivityTemplate));
                //.ForMember(a => a.ActivityTemplate, opts => opts.Ignore());

            Mapper.CreateMap<ActionDTO, ActionDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.ParentActivityId, opts => opts.ResolveUsing(ad => ad.ActionListId))
                .ForMember(a => a.ActivityTemplateId, opts => opts.ResolveUsing(ad => ad.ActivityTemplateId))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(ad => ad.ActivityTemplate))
                .ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.SerializeObject(ad.CrateStorage)))
                .ForMember(a => a.currentView, opts => opts.ResolveUsing(ad => ad.CurrentView))
                .ForMember(a => a.IsTempId, opts => opts.ResolveUsing(ad => ad.IsTempId));

            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.PluginID, opts => opts.ResolveUsing(x => x.PluginID)); ;

            Mapper.CreateMap<ActivityTemplateDTO, ActivityTemplateDO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.ComponentActivities, opts => opts.ResolveUsing(x => x.ComponentActivities))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.PluginID, opts => opts.ResolveUsing(x => x.PluginID))
                .ForMember(x => x.Plugin, opts => opts.ResolveUsing((ActivityTemplateDTO x) => null));

            Mapper.CreateMap<ActionListDO, ActionListDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.ActionListType, opts => opts.ResolveUsing(x => x.ActionListType))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name));

            Mapper.CreateMap<ProcessTemplateDO, ProcessTemplateOnlyDTO>();

            Mapper.CreateMap<ProcessNodeTemplateDTO, ProcessNodeTemplateDO>()
                .ForMember(x => x.ParentTemplateId, opts => opts.ResolveUsing(x => x.ProcessTemplateId));
            Mapper.CreateMap<ProcessNodeTemplateDO, ProcessNodeTemplateDTO>()
                .ForMember(x => x.ProcessTemplateId, opts => opts.ResolveUsing(x => x.ParentTemplateId));

            Mapper.CreateMap<CriteriaDO, CriteriaDTO>()
                .ForMember(x => x.Conditions, opts => opts.ResolveUsing(y => y.ConditionsJSON));
            Mapper.CreateMap<CriteriaDTO, CriteriaDO>()
                .ForMember(x => x.ConditionsJSON, opts => opts.ResolveUsing(y => y.Conditions));

            Mapper.CreateMap<ProcessTemplateDO, ProcessTemplateDTO>()
                .ConvertUsing<ProcessTemplateDOFullConverter>();

            Mapper.CreateMap<ProcessTemplateOnlyDTO, ProcessTemplateDTO>();
            Mapper.CreateMap<ActionListDO, FullActionListDTO>();
            Mapper.CreateMap<ProcessNodeTemplateDO, FullProcessNodeTemplateDTO>();

            //Mapper.CreateMap<Account, DocuSignAccount>();
            Mapper.CreateMap<FileDO, FileDescriptionDTO>();

            Mapper.CreateMap<CrateStorageDTO, string>()
                .ConvertUsing<JSONToStringConverter<CrateStorageDTO>>();
            Mapper.CreateMap<string, CrateStorageDTO>()
                .ConvertUsing<StringToJSONConverter<CrateStorageDTO>>();
            Mapper.CreateMap<FileDO, FileDTO>();

        }
    }   
}