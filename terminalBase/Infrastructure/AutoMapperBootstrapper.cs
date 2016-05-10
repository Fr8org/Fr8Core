using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using StructureMap;
using TerminalBase.BaseClasses;

namespace TerminalBase.Infrastructure
{
    public class AutoMapperBootstrapper
    {
        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<AuthorizationTokenDTO, AuthorizationToken>();
            Mapper.CreateMap<ActivityDTO, ActivityPayload>()
                .ForMember(x => x.ChildrenActivities, opts => opts.MapFrom(x => x.ChildrenActivities))
                .ForMember(x => x.CrateStorage, opts => opts.ResolveUsing(CrateStorageResolver));

            Mapper.CreateMap<ActivityPayload, ActivityDTO>()
                .ForMember(x => x.ChildrenActivities, opts => opts.MapFrom(x => x.ChildrenActivities))
                .ForMember(x => x.CrateStorage, opts => opts.ResolveUsing(CrateStorageDTOResolver));

            Mapper.CreateMap<Fr8DataDTO, ActivityContext>()
                .ForMember(x => x.UserId, opts => opts.MapFrom(x => x.ActivityDTO.AuthToken.UserId))
                .ForMember(x => x.AuthorizationToken, opts => opts.MapFrom(x => x.ActivityDTO.AuthToken))
                .ForMember(x => x.ActivityPayload, opts => opts.MapFrom(x => x.ActivityDTO));
        }

        public static ICrateStorage CrateStorageResolver(ActivityDTO activityDTO)
        {
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            return crateManager.GetUpdatableStorage(activityDTO);
        }

        public static CrateStorageDTO CrateStorageDTOResolver(ActivityPayload activityPayload)
        {
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            return crateManager.ToDto(activityPayload.CrateStorage);
        }

    }
}
