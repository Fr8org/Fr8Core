using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Core.PluginRegistrations
{
    public class AutoMapperBootStrapper
    {
        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<ActionDO, ActionDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.UserLabel, opts => opts.ResolveUsing(ad => ad.UserLabel))
                .ForMember(a => a.ActionType, opts => opts.ResolveUsing(ad => ad.ActionType))
                .ForMember(a => a.ActionListId, opts => opts.ResolveUsing(ad => ad.ActionListId))
                .ForMember(a => a.ConfigurationSettings, opts => opts.ResolveUsing(ad => ad.ConfigurationSettings))
                .ForMember(a => a.FieldMappingSettings, opts => opts.ResolveUsing(ad => ad.FieldMappingSettings))
                .ForMember(a => a.ParentPluginRegistration, opts => opts.ResolveUsing(ad => ad.ParentPluginRegistration));
        }
    }
}