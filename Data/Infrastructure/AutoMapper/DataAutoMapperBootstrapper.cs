using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Utilities.AutoMapper;
using Data.Interfaces.Manifests;
using Data.Interfaces.DataTransferObjects.PlanDescription;

namespace Data.Infrastructure.AutoMapper
{
    public class DataAutoMapperBootStrapper
    {
        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<ActivityNameDTO, ActivityTemplateDO>()
                .ForMember(activityTemplateDO => activityTemplateDO.Name, opts => opts.ResolveUsing(e => e.Name))
                .ForMember(activityTemplateDO => activityTemplateDO.Version, opts => opts.ResolveUsing(e => e.Version));


            Mapper.CreateMap<ActivityDO, ActivityDTO>();
            Mapper.CreateMap<FactDO, FactDTO>();

            Mapper.CreateMap<Fr8AccountDO, UserDTO>()
                .ForMember(dto => dto.EmailAddress, opts => opts.ResolveUsing(e => e.EmailAddress.Address))
                .ForMember(dto => dto.Status, opts => opts.ResolveUsing(e => e.State.Value));

            Mapper.CreateMap<WebServiceDO, WebServiceDTO>();
            Mapper.CreateMap<WebServiceDTO, WebServiceDO>()
                .ForMember(x => x.LastUpdated, opts => opts.Ignore())
                .ForMember(x => x.CreateDate, opts => opts.Ignore());
            Mapper.CreateMap<string, JToken>().ConvertUsing<StringToJTokenConverter>();
            Mapper.CreateMap<JToken, string>().ConvertUsing<JTokenToStringConverter>();

            Mapper.CreateMap<ActivityDO, ActivityDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.RootPlanNodeId, opts => opts.ResolveUsing(ad => ad.RootPlanNodeId))
                .ForMember(a => a.ParentPlanNodeId, opts => opts.ResolveUsing(ad => ad.ParentPlanNodeId))
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => ad.CrateStorage == null ? null : JsonConvert.DeserializeObject(ad.CrateStorage)))
                .ForMember(a => a.CurrentView, opts => opts.ResolveUsing(ad => ad.currentView))
                .ForMember(a => a.ChildrenActivities, opts => opts.ResolveUsing(ad => ad.ChildNodes.OfType<ActivityDO>().OrderBy(da => da.Ordering)))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(ad => ad.ActivityTemplate))
                .ForMember(a => a.AuthToken, opts => opts.ResolveUsing(ad => ad.AuthorizationToken))
                .ForMember(a => a.Fr8AccountId, opts => opts.ResolveUsing(ad => ad.Fr8AccountId));


            Mapper.CreateMap<ActivityDTO, ActivityDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.RootPlanNodeId, opts => opts.ResolveUsing(ad => ad.RootPlanNodeId))
                .ForMember(a => a.ParentPlanNodeId, opts => opts.ResolveUsing(ad => ad.ParentPlanNodeId))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(ad => ad.ActivityTemplate))
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.SerializeObject(ad.CrateStorage)))
                .ForMember(a => a.currentView, opts => opts.ResolveUsing(ad => ad.CurrentView))
                .ForMember(a => a.ChildNodes, opts => opts.ResolveUsing(ad => MapActivities(ad.ChildrenActivities)))
                .ForMember(a => a.AuthorizationTokenId, opts => opts.ResolveUsing(ad => ad.AuthToken != null && ad.AuthToken.Id != null ? new Guid(ad.AuthToken.Id) : (Guid?)null))
                .ForMember(a => a.Fr8AccountId, opts => opts.ResolveUsing(ad => ad.Fr8AccountId));


            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication));

            Mapper.CreateMap<ActivityTemplateDTO, ActivityTemplateDO>()
                .ConstructUsing((Func<ResolutionContext, ActivityTemplateDO>)(r => new ActivityTemplateDO()))
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.Terminal, opts => opts.ResolveUsing(x => x.Terminal))
                // .ForMember(x => x.AuthenticationType, opts => opts.ResolveUsing(x => x.AuthenticationType))
                .ForMember(x => x.WebService, opts => opts.ResolveUsing(x => Mapper.Map<WebServiceDO>(x.WebService)))
                // .ForMember(x => x.AuthenticationTypeTemplate, opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication))
                .ForMember(x => x.ActivityTemplateStateTemplate,
                    opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.WebServiceId, opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.ComponentActivities, opts => opts.Ignore())
                .ForMember(x => x.ActivityTemplateState, opts => opts.Ignore())
                .ForMember(x => x.TerminalId, opts => opts.Ignore())
                .ForMember(x => x.LastUpdated, opts => opts.Ignore())
                .ForMember(x => x.CreateDate, opts => opts.Ignore());

            //
            //            Mapper.CreateMap<ActionListDO, ActionListDTO>()
            //                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
            //                .ForMember(x => x.ActionListType, opts => opts.ResolveUsing(x => x.ActionListType))
            //                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name));

            Mapper.CreateMap<PlanDO, PlanEmptyDTO>();
            Mapper.CreateMap<PlanEmptyDTO, PlanDO>();
            Mapper.CreateMap<PlanDO, PlanEmptyDTO>();
            Mapper.CreateMap<SubPlanDTO, SubPlanDO>()
                .ForMember(x => x.Name, opts => opts.MapFrom(e => e.Name))
                .ForMember(x => x.NodeTransitions, opts => opts.MapFrom(e => e.TransitionKey))
                .ForMember(x => x.Id, opts => opts.MapFrom(e => e.SubPlanId ?? Guid.Empty))
                .ForMember(x => x.ParentPlanNodeId, opts => opts.MapFrom(e => e.ParentId))
                .ForMember(x => x.RootPlanNodeId, opts => opts.MapFrom(e => e.PlanId))
                .ForMember(x => x.StartingSubPlan, opts => opts.UseValue(false))
                .ForMember(x => x.RootPlanNode, opts => opts.Ignore())
                .ForMember(x => x.ParentPlanNode, opts => opts.Ignore())
                .ForMember(x => x.ChildNodes, opts => opts.Ignore())
                .ForMember(x => x.Fr8AccountId, opts => opts.Ignore())
                .ForMember(x => x.Fr8Account, opts => opts.Ignore())
                .ForMember(x => x.Ordering, opts => opts.Ignore())
                .ForMember(x => x.LastUpdated, opts => opts.Ignore())
                .ForMember(x => x.CreateDate, opts => opts.Ignore());

            Mapper.CreateMap<SubPlanDO, SubPlanDTO>()
                .ForMember(x => x.Name, opts => opts.ResolveUsing(e => e.Name))
                .ForMember(x => x.TransitionKey, opts => opts.ResolveUsing(e => e.NodeTransitions))
                .ForMember(x => x.SubPlanId, opts => opts.ResolveUsing(e => e.Id))
                .ForMember(x => x.PlanId, opts => opts.ResolveUsing(e => e.RootPlanNodeId));

            Mapper.CreateMap<SubPlanDO, SubPlanDTO>()
                .ForMember(x => x.ParentId, opts => opts.ResolveUsing(x => x.ParentPlanNodeId))
                .ForMember(x => x.PlanId, opts => opts.ResolveUsing(x => x.RootPlanNodeId))
                .ForMember(x => x.SubPlanId, opts => opts.ResolveUsing(x => x.Id));

            Mapper.CreateMap<CriteriaDO, CriteriaDTO>()
                .ForMember(x => x.Conditions, opts => opts.ResolveUsing(y => y.ConditionsJSON));
            Mapper.CreateMap<CriteriaDTO, CriteriaDO>()
                .ForMember(x => x.ConditionsJSON, opts => opts.ResolveUsing(y => y.Conditions));

            Mapper.CreateMap<PlanDO, PlanFullDTO>()
                .ConvertUsing<PlanDOFullConverter>();

            Mapper.CreateMap<PlanEmptyDTO, PlanFullDTO>();


            //  Mapper.CreateMap<ActionListDO, FullActionListDTO>();
            Mapper.CreateMap<SubPlanDO, FullSubPlanDTO>()
                .ForMember(x => x.SubPlanId, opts => opts.ResolveUsing(x => x.Id));

            Mapper.CreateMap<FullSubPlanDTO, SubPlanDO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.SubPlanId));


            //Mapper.CreateMap<Account, DocuSignAccount>();
            Mapper.CreateMap<FileDO, FileDescriptionDTO>();

            Mapper.CreateMap<CrateStorageDTO, string>()
                .ConvertUsing<JsonToStringConverterNoMagic<CrateStorageDTO>>();
            Mapper.CreateMap<string, CrateStorageDTO>()
                .ConvertUsing<CrateStorageFromStringConverter>();
            Mapper.CreateMap<FileDO, FileDTO>();

            Mapper.CreateMap<ContainerDO, ContainerDTO>()
                .ForMember(
                    x => x.CurrentActivityResponse,
                    x => x.ResolveUsing(y => ExtractOperationStateData(y, z => z.CurrentActivityResponse != null ? Enum.Parse(typeof(Data.Constants.ActivityResponse), z.CurrentActivityResponse.Type) : null))
                )
                .ForMember(
                    x => x.CurrentClientActivityName,
                    x => x.ResolveUsing(y => ExtractOperationStateData(y, z => z.CurrentClientActivityName))
                );
            Mapper.CreateMap<AuthorizationTokenDTO, AuthorizationTokenDO>()
                .ForMember(x => x.UserID, x => x.ResolveUsing(y => y.UserId))
                .ForMember(x => x.Id, x => x.ResolveUsing(y => y.Id != null ? new Guid(y.Id) : (Guid?)null));
            Mapper.CreateMap<AuthorizationTokenDO, AuthorizationTokenDTO>()
                .ForMember(x => x.UserId, x => x.ResolveUsing(y => y.UserID))
                .ForMember(x => x.Id, x => x.ResolveUsing(y => y.Id.ToString()));

            Mapper.CreateMap<ManifestDescriptionCM, ManifestDescriptionDTO>();
            Mapper.CreateMap<ManifestDescriptionDTO, ManifestDescriptionCM>();


            Mapper.CreateMap<TerminalDO, TerminalDTO>();
            Mapper.CreateMap<TerminalDTO, TerminalDO>()
                .ForMember(x => x.LastUpdated, opts => opts.Ignore())
                .ForMember(x => x.CreateDate, opts => opts.Ignore())
                .ForMember(x => x.Id, opts => opts.Ignore())
                .ForMember(x => x.PublicIdentifier, opts => opts.Ignore())
                .ForMember(x => x.Secret, opts => opts.Ignore())
                .ForMember(x => x.AuthenticationTypeTemplate, opts => opts.Ignore());


            Mapper.CreateMap<PlanDescriptionDO, PlanDescriptionDTO>();
            Mapper.CreateMap<PlanDescriptionDTO, PlanDescriptionDTO>();

            Mapper.CreateMap<PlanNodeDescriptionDO, PlanNodeDescriptionDTO>();
            Mapper.CreateMap<PlanNodeDescriptionDTO, PlanNodeDescriptionDO>();

            Mapper.CreateMap<ActivityDescriptionDO, ActivityDescriptionDTO>();
            Mapper.CreateMap<ActivityDescriptionDTO, ActivityDescriptionDO>();

            Mapper.CreateMap<ActivityTransitionDO, ActivityTransitionDTO>();
            Mapper.CreateMap<ActivityTransitionDTO, ActivityTransitionDO>();
        }

        private static List<PlanNodeDO> MapActivities(IEnumerable<ActivityDTO> actions)
        {
            var list = new List<PlanNodeDO>();

            if (actions != null)
            {
                foreach (var activityDto in actions)
                {
                    list.Add(Mapper.Map<ActivityDO>(activityDto));
                }
            }

            return list;
        }

        private static object ExtractOperationStateData(
            ContainerDO container,
            Func<OperationalStateCM, object> extractor)
        {
            var crateStorageDTO = CrateStorageFromStringConverter.Convert(container.CrateStorage);
            var crateStorage = CrateStorageSerializer.Default.ConvertFromDto(crateStorageDTO);
            var cm = crateStorage
                .CrateContentsOfType<OperationalStateCM>()
                .SingleOrDefault();

            if (cm == null)
            {
                return null;
            }

            return extractor(cm);
        }
    }
}