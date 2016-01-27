using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Utilities.AutoMapper;
using Signer = DocuSign.Integrations.Client.Signer;

namespace Data.Infrastructure.AutoMapper
{
    public class DataAutoMapperBootStrapper
    {
        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<ActionNameDTO, ActivityTemplateDO>()
                .ForMember(activityTemplateDO => activityTemplateDO.Name, opts => opts.ResolveUsing(e => e.Name))
                .ForMember(activityTemplateDO => activityTemplateDO.Version, opts => opts.ResolveUsing(e => e.Version));


            Mapper.CreateMap<ActivityDO, ActivityDTO>();
            Mapper.CreateMap<FactDO, FactDTO>();

            Mapper.CreateMap<Fr8AccountDO, UserDTO>()
                .ForMember(dto => dto.EmailAddress, opts => opts.ResolveUsing(e => e.EmailAddress.Address))
                .ForMember(dto => dto.Status, opts => opts.ResolveUsing(e => e.State.Value));

            Mapper.CreateMap<WebServiceDO, WebServiceDTO>();
            Mapper.CreateMap<WebServiceDTO, WebServiceDO>();
            Mapper.CreateMap<string, JToken>().ConvertUsing<StringToJTokenConverter>();
            Mapper.CreateMap<JToken, string>().ConvertUsing<JTokenToStringConverter>();

            Mapper.CreateMap<ActivityDO, ActivityDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.RootRouteNodeId, opts => opts.ResolveUsing(ad => ad.RootRouteNodeId))
                .ForMember(a => a.ParentRouteNodeId, opts => opts.ResolveUsing(ad => ad.ParentRouteNodeId))
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => ad.CrateStorage == null ? null : JsonConvert.DeserializeObject(ad.CrateStorage)))
                .ForMember(a => a.ActivityTemplateId, opts => opts.ResolveUsing(ad => ad.ActivityTemplateId))
                .ForMember(a => a.CurrentView, opts => opts.ResolveUsing(ad => ad.currentView))
                .ForMember(a => a.ChildrenActions, opts => opts.ResolveUsing(ad => ad.ChildNodes.OfType<ActivityDO>().OrderBy(da => da.Ordering)))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(ad => ad.ActivityTemplate))
                .ForMember(a => a.ExplicitData, opts => opts.ResolveUsing(ad => ad.ExplicitData))
                .ForMember(a => a.AuthToken, opts => opts.ResolveUsing(ad => ad.AuthorizationToken))
                .ForMember(a => a.Fr8AccountId, opts => opts.ResolveUsing(ad => ad.Fr8AccountId));


            Mapper.CreateMap<ActivityDTO, ActivityDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.RootRouteNodeId, opts => opts.ResolveUsing(ad => ad.RootRouteNodeId))
                .ForMember(a => a.ParentRouteNodeId, opts => opts.ResolveUsing(ad => ad.ParentRouteNodeId))
                .ForMember(a => a.ActivityTemplateId, opts => opts.ResolveUsing(ad => ad.ActivityTemplateId))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(ad => ad.ActivityTemplate))
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.SerializeObject(ad.CrateStorage)))
                .ForMember(a => a.currentView, opts => opts.ResolveUsing(ad => ad.CurrentView))
                .ForMember(a => a.ChildNodes, opts => opts.ResolveUsing(ad => MapActions(ad.ChildrenActions)))
                .ForMember(a => a.IsTempId, opts => opts.ResolveUsing(ad => ad.IsTempId))
                .ForMember(a => a.ExplicitData, opts => opts.ResolveUsing(ad => ad.ExplicitData))
                .ForMember(a => a.AuthorizationTokenId, opts => opts.ResolveUsing(ad => ad.AuthToken != null && ad.AuthToken.Id != null ? new Guid(ad.AuthToken.Id) : (Guid?)null))
                .ForMember(a => a.Fr8AccountId, opts => opts.ResolveUsing(ad => ad.Fr8AccountId));


            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.Description, opts => opts.ResolveUsing(x => x.Description))
                .ForMember(x => x.TerminalId, opts => opts.ResolveUsing(x => x.TerminalId))
                .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication));

            Mapper.CreateMap<ActivityTemplateDTO, ActivityTemplateDO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.ComponentActivities, opts => opts.ResolveUsing(x => x.ComponentActivities))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.TerminalId, opts => opts.ResolveUsing(x => x.TerminalId))
                .ForMember(x => x.Terminal, opts => opts.ResolveUsing(x => x.Terminal))
                // .ForMember(x => x.AuthenticationType, opts => opts.ResolveUsing(x => x.AuthenticationType))
                .ForMember(x => x.WebService, opts => opts.ResolveUsing(x => Mapper.Map<WebServiceDO>(x.WebService)))
                // .ForMember(x => x.AuthenticationTypeTemplate, opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication))
                .ForMember(x => x.ActivityTemplateStateTemplate,
                    opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.WebServiceId, opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.Description, opts => opts.ResolveUsing(x => x.Description));

            //
            //            Mapper.CreateMap<ActionListDO, ActionListDTO>()
            //                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
            //                .ForMember(x => x.ActionListType, opts => opts.ResolveUsing(x => x.ActionListType))
            //                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name));

            Mapper.CreateMap<PlanDO, RouteEmptyDTO>();
            Mapper.CreateMap<RouteEmptyDTO, PlanDO>();
            Mapper.CreateMap<PlanDO, RouteEmptyDTO>();
            Mapper.CreateMap<SubrouteDTO, SubrouteDO>()
                .ForMember(x => x.ParentRouteNodeId, opts => opts.ResolveUsing(x => x.RouteId))
                .ForMember(x => x.RootRouteNodeId, opts => opts.ResolveUsing(x => x.RouteId));
            Mapper.CreateMap<SubrouteDO, SubrouteDTO>()
                .ForMember(x => x.RouteId, opts => opts.ResolveUsing(x => x.ParentRouteNodeId))
                .ForMember(x => x.RouteId, opts => opts.ResolveUsing(x => x.RootRouteNodeId));

            Mapper.CreateMap<CriteriaDO, CriteriaDTO>()
                .ForMember(x => x.Conditions, opts => opts.ResolveUsing(y => y.ConditionsJSON));
            Mapper.CreateMap<CriteriaDTO, CriteriaDO>()
                .ForMember(x => x.ConditionsJSON, opts => opts.ResolveUsing(y => y.Conditions));

            Mapper.CreateMap<PlanDO, RouteFullDTO>()
                .ConvertUsing<RouteDOFullConverter>();

            Mapper.CreateMap<RouteEmptyDTO, RouteFullDTO>();
            //  Mapper.CreateMap<ActionListDO, FullActionListDTO>();
            Mapper.CreateMap<SubrouteDO, FullSubrouteDTO>();

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
                    x => x.ResolveUsing(y => ExtractOperationStateData(y, z => z.CurrentActivityResponse))
                )
                .ForMember(
                    x => x.CurrentClientActionName,
                    x => x.ResolveUsing(y => ExtractOperationStateData(y, z => z.CurrentClientActionName))
                );
            Mapper.CreateMap<AuthorizationTokenDTO, AuthorizationTokenDO>()
                .ForMember(x => x.UserID, x => x.ResolveUsing(y => y.UserId))
                .ForMember(x => x.Id, x => x.ResolveUsing(y => y.Id != null ? new Guid(y.Id) : (Guid?)null));
            Mapper.CreateMap<AuthorizationTokenDO, AuthorizationTokenDTO>()
                .ForMember(x => x.UserId, x => x.ResolveUsing(y => y.UserID))
                .ForMember(x => x.Id, x => x.ResolveUsing(y => y.Id.ToString()));

            Mapper.CreateMap<TerminalDO, TerminalDTO>();
            Mapper.CreateMap<TerminalDTO, TerminalDO>();
        }

        private static List<RouteNodeDO> MapActions(IEnumerable<ActivityDTO> actions)
        {
            var list = new List<RouteNodeDO>();

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