using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Infrastructure
{
    public class AutoMapperBootstrapper
    {
        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<AuthorizationTokenDTO, AuthorizationToken>()
                .ForMember(x => x.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(x => x.Token, opts => opts.MapFrom(src => src.Token))
                .ForMember(x => x.ExternalAccountId, opts => opts.MapFrom(src => src.ExternalAccountId))
                .ForMember(x => x.ExternalAccountName, opts => opts.MapFrom(src => src.ExternalAccountName))
                .ForMember(x => x.ExternalDomainId, opts => opts.MapFrom(src => src.ExternalDomainId))
                .ForMember(x => x.ExternalDomainName, opts => opts.MapFrom(src => src.ExternalDomainName))
                .ForMember(x => x.UserId, opts => opts.MapFrom(src => src.UserId))
                .ForMember(x => x.ExternalStateToken, opts => opts.MapFrom(src => src.ExternalStateToken))
                .ForMember(x => x.ExpiresAt, opts => opts.MapFrom(src => src.ExpiresAt))
                .ForMember(x => x.AdditionalAttributes, opts => opts.MapFrom(src => src.AdditionalAttributes))
                .ForMember(x => x.Error, opts => opts.MapFrom(src => src.Error))
                .ForMember(x => x.AuthCompletedNotificationRequired, opts => opts.MapFrom(src => src.AuthCompletedNotificationRequired));

            Mapper.CreateMap<AuthorizationToken, AuthorizationTokenDTO>()
                .ForMember(x => x.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(x => x.Token, opts => opts.MapFrom(src => src.Token))
                .ForMember(x => x.ExternalAccountId, opts => opts.MapFrom(src => src.ExternalAccountId))
                .ForMember(x => x.ExternalAccountName, opts => opts.MapFrom(src => src.ExternalAccountName))
                .ForMember(x => x.ExternalDomainId, opts => opts.MapFrom(src => src.ExternalDomainId))
                .ForMember(x => x.ExternalDomainName, opts => opts.MapFrom(src => src.ExternalDomainName))
                .ForMember(x => x.UserId, opts => opts.MapFrom(src => src.UserId))
                .ForMember(x => x.ExternalStateToken, opts => opts.MapFrom(src => src.ExternalStateToken))
                .ForMember(x => x.ExpiresAt, opts => opts.MapFrom(src => src.ExpiresAt))
                .ForMember(x => x.AdditionalAttributes, opts => opts.MapFrom(src => src.AdditionalAttributes))
                .ForMember(x => x.Error, opts => opts.MapFrom(src => src.Error))
                .ForMember(x => x.AuthCompletedNotificationRequired, opts => opts.MapFrom(src => src.AuthCompletedNotificationRequired));


            Mapper.CreateMap<ActivityDTO, ActivityPayload>()
                .ForMember(x => x.ChildrenActivities, opts => opts.MapFrom(src => src.ChildrenActivities != null ? src.ChildrenActivities.ToList() : new List<ActivityDTO>()))
                .ForMember(x => x.CrateStorage, opts => opts.Ignore())
                .AfterMap((activityDTO, activityPayload) =>
                {
                    //there are some mapping inheritance problems in automapper
                    //that is why i am solving this on afterMap
                    //TODO inspect this
                    activityPayload.CrateStorage = GetCrateStorage(activityDTO);
                });
            
            Mapper.CreateMap<ActivityPayload, ActivityDTO>()
                .ForMember(x => x.ChildrenActivities, opts => opts.MapFrom(x => x.ChildrenActivities.ToArray()))
                .ForMember(x => x.CrateStorage, opts => opts.ResolveUsing(CrateStorageDTOResolver));

            Mapper.CreateMap<Fr8DataDTO, ActivityContext>()
                .ForMember(x => x.UserId, opts => opts.MapFrom(x => x.ActivityDTO.AuthToken.UserId))
                .ForMember(x => x.AuthorizationToken,opts => opts.MapFrom(x => x.ActivityDTO.AuthToken))
                .ForMember(x => x.ActivityPayload, opts => opts.MapFrom(x => x.ActivityDTO));

            Mapper.CreateMap<ContainerExecutionContext, PayloadDTO>()
                .ForMember(x => x.ContainerId, opts => opts.MapFrom(src => src.ContainerId))
                .ForMember(x => x.CrateStorage, opts => opts.ResolveUsing(CrateStorageDTOResolver));
        }

        public static ICrateStorage GetCrateStorage(ActivityDTO activityDTO)
        {
            return CrateStorageSerializer.Default.ConvertFromDto(activityDTO?.CrateStorage);
        }

        public static CrateStorageDTO CrateStorageDTOResolver(ContainerExecutionContext executionContext)
        {
            return CrateStorageSerializer.Default.ConvertToDto(executionContext.PayloadStorage);
        }

        public static CrateStorageDTO CrateStorageDTOResolver(ActivityPayload activityPayload)
        {
            return CrateStorageSerializer.Default.ConvertToDto(activityPayload.CrateStorage);
        }
    }
}
