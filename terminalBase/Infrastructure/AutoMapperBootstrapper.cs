using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using StructureMap;
using TerminalBase.Models;

namespace TerminalBase.Infrastructure
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
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            return crateManager.GetStorage(activityDTO);
        }

        public static CrateStorageDTO CrateStorageDTOResolver(ContainerExecutionContext executionContext)
        {
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            return crateManager.ToDto(executionContext.PayloadStorage);
        }

        public static CrateStorageDTO CrateStorageDTOResolver(ActivityPayload activityPayload)
        {
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            return crateManager.ToDto(activityPayload.CrateStorage);
        }
    }
}
