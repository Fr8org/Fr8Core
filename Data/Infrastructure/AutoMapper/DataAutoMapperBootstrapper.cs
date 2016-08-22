using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.AutoMapper;
using Newtonsoft.Json.Linq;

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
            Mapper.CreateMap<FactDO, HistoryItemDTO>();
            Mapper.CreateMap<IncidentDO, HistoryItemDTO>();
            Mapper.CreateMap<IncidentDO, IncidentDTO>();
            Mapper.CreateMap<FactDO, FactDTO>();

            Mapper.CreateMap<OrganizationDTO, OrganizationDO>();
            Mapper.CreateMap<OrganizationDO, OrganizationDTO>();

            Mapper.CreateMap<Fr8AccountDO, UserDTO>()
                .ForMember(dto => dto.EmailAddress, opts => opts.ResolveUsing(e => e.EmailAddress.Address))
                .ForMember(dto => dto.Status, opts => opts.ResolveUsing(e => e.State.Value));

            Mapper.CreateMap<string, JToken>().ConvertUsing<StringToJTokenConverter>();
            Mapper.CreateMap<JToken, string>().ConvertUsing<JTokenToStringConverter>();

            Mapper.CreateMap<ActivityDO, ActivityDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.RootPlanNodeId, opts => opts.ResolveUsing(ad => ad.RootPlanNodeId))
                .ForMember(a => a.ParentPlanNodeId, opts => opts.ResolveUsing(ad => ad.ParentPlanNodeId))
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => ad.CrateStorage == null ? null : JsonConvert.DeserializeObject(ad.CrateStorage)))
                .ForMember(a => a.ChildrenActivities, opts => opts.ResolveUsing(ad => ad.ChildNodes.OfType<ActivityDO>().OrderBy(da => da.Ordering)))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(ad => ad.ActivityTemplate))
                .ForMember(a => a.AuthToken, opts => opts.ResolveUsing(ad => ad.AuthorizationToken))
                .ForMember(a => a.AuthTokenId, opts => opts.ResolveUsing(ad => ad.AuthorizationTokenId));


            Mapper.CreateMap<ActivityDTO, ActivityDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.RootPlanNodeId, opts => opts.ResolveUsing(ad => ad.RootPlanNodeId))
                .ForMember(a => a.ParentPlanNodeId, opts => opts.ResolveUsing(ad => ad.ParentPlanNodeId))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(dto => new ActivityTemplateDO
                {
                    Name = dto.ActivityTemplate.Name,
                    Version = dto.ActivityTemplate.Version,
                    Terminal = new TerminalDO
                    {
                        Name = dto.ActivityTemplate.TerminalName,
                        Version = dto.ActivityTemplate.TerminalVersion
                    }
                }))
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.SerializeObject(ad.CrateStorage)))
                .ForMember(a => a.ChildNodes, opts => opts.ResolveUsing(ad => MapActivities(ad.ChildrenActivities)))
                .ForMember(a => a.AuthorizationTokenId, opts => opts.ResolveUsing(ad => ad.AuthToken != null && ad.AuthToken.Id != null ? new Guid(ad.AuthToken.Id) : (Guid?) null));

            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateSummaryDTO>()
               .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
               .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
               .ForMember(x => x.TerminalName, opts => opts.ResolveUsing(x => x.Terminal.Name))
               .ForMember(x => x.TerminalVersion, opts => opts.ResolveUsing(x => x.Terminal.Version));

            Mapper.CreateMap<ActivityCategoryDO, ActivityCategoryDTO>();
            Mapper.CreateMap<ActivityCategoryDTO, ActivityCategoryDO>();

            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateDTO>()
                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication))
                .ForMember(x => x.ShowDocumentation, opts => opts.Ignore())
                .ForMember(x => x.Description, opts => opts.ResolveUsing(x => x.Description))
                .ForMember(
                    x => x.Categories,
                    opts => opts.ResolveUsing((ActivityTemplateDO x) =>
                        x.Categories != null
                        ? x.Categories
                            .Where(y => y.ActivityCategory != null)
                            .Select(y => new ActivityCategoryDTO()
                            {
                                Id = y.ActivityCategory.Id,
                                Name = y.ActivityCategory.Name,
                                IconPath = y.ActivityCategory.IconPath,
                                Type=y.ActivityCategory.Type
                            })
                        : new List<ActivityCategoryDTO>()
                    )
                );

            Mapper.CreateMap<ActivityTemplateDTO, ActivityTemplateDO>()
                .ConstructUsing((Func<ResolutionContext, ActivityTemplateDO>)(r => new ActivityTemplateDO()))
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
                .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
                .ForMember(x => x.Terminal, opts => opts.ResolveUsing(x => x.Terminal))
                // .ForMember(x => x.AuthenticationType, opts => opts.ResolveUsing(x => x.AuthenticationType))                
                // .ForMember(x => x.AuthenticationTypeTemplate, opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication))
                .ForMember(x => x.ActivityTemplateStateTemplate,
                    opts => opts.ResolveUsing((ActivityTemplateDTO x) => null))
                .ForMember(x => x.ActivityTemplateState, opts => opts.Ignore())
                .ForMember(x => x.TerminalId, opts => opts.Ignore())
                .ForMember(x => x.LastUpdated, opts => opts.Ignore())
                .ForMember(x => x.CreateDate, opts => opts.Ignore())
                .ForMember(
                    x => x.Categories,
                    opts => opts.ResolveUsing((ActivityTemplateDTO x) =>
                        x.Categories != null
                        ? x.Categories
                            .Select(y => new ActivityCategorySetDO()
                            {
                                ActivityCategory = new ActivityCategoryDO()
                                {
                                    Id = y.Id,
                                    Name = y.Name,
                                    IconPath = y.IconPath,
                                    Type=y.Type
                                }
                            })
                            .ToList()
                        : null
                    )
                );

            //
            //            Mapper.CreateMap<ActionListDO, ActionListDTO>()
            //                .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
            //                .ForMember(x => x.ActionListType, opts => opts.ResolveUsing(x => x.ActionListType))
            //                .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name));

            Mapper.CreateMap<PlanDO, PlanNoChildrenDTO>();
            Mapper.CreateMap<PlanNoChildrenDTO, PlanDO>();
            Mapper.CreateMap<PlanDO, PlanNoChildrenDTO>();
            Mapper.CreateMap<SubplanDTO, SubplanDO>()
                .ForMember(x => x.Name, opts => opts.MapFrom(e => e.Name))
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

            Mapper.CreateMap<SubplanDO, SubplanDTO>()
                .ForMember(x => x.Name, opts => opts.ResolveUsing(e => e.Name))
                .ForMember(x => x.SubPlanId, opts => opts.ResolveUsing(e => e.Id))
                .ForMember(x => x.PlanId, opts => opts.ResolveUsing(e => e.RootPlanNodeId));

            Mapper.CreateMap<SubplanDO, SubplanDTO>()
                .ForMember(x => x.ParentId, opts => opts.ResolveUsing(x => x.ParentPlanNodeId))
                .ForMember(x => x.PlanId, opts => opts.ResolveUsing(x => x.RootPlanNodeId))
                .ForMember(x => x.SubPlanId, opts => opts.ResolveUsing(x => x.Id));

            Mapper.CreateMap<PlanDO, PlanDTO>().ConvertUsing<PlanDOFullConverter>();

            Mapper.CreateMap<PlanNoChildrenDTO, PlanDTO>();


            //  Mapper.CreateMap<ActionListDO, FullActionListDTO>();
            Mapper.CreateMap<SubplanDO, FullSubplanDto>()
                .ForMember(x => x.SubPlanId, opts => opts.ResolveUsing(x => x.Id));

            Mapper.CreateMap<FullSubplanDto, SubplanDO>()
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
                    x => x.ResolveUsing(y => ExtractOperationStateData(y, z => z.CurrentActivityResponse != null ? Enum.Parse(typeof(Fr8.Infrastructure.Data.Constants.ActivityResponse), z.CurrentActivityResponse.Type) : null))
                )
                .ForMember(
                    x => x.CurrentClientActivityName,
                    x => x.ResolveUsing(y => ExtractOperationStateData(y, z => z.CurrentActivityResponse?.Body))
                );
            Mapper.CreateMap<AuthorizationTokenDTO, AuthorizationTokenDO>()
                  .ForMember(x => x.UserID, x => x.ResolveUsing(y => y.UserId))
                  .ForMember(x => x.Id, x => x.ResolveUsing(y => y.Id != null ? new Guid(y.Id) : (Guid?)null))
                  .ForMember(x => x.ExternalDomainId, x => x.ResolveUsing(y => y.ExternalDomainId))
                  .ForMember(x => x.ExpiresAt, x => x.ResolveUsing(y => y.ExpiresAt));
            
            Mapper.CreateMap<AuthorizationTokenDO, AuthorizationTokenDTO>()
                .ForMember(x => x.UserId, x => x.ResolveUsing(y => y.UserID))
                .ForMember(x => x.Id, x => x.ResolveUsing(y => y.Id.ToString()))
                .ForMember(x => x.ExternalDomainId, x => x.ResolveUsing(y => y.ExternalDomainId))
                .ForMember(x => x.ExpiresAt, x => x.ResolveUsing(y => y.ExpiresAt));

            Mapper.CreateMap<ManifestDescriptionCM, ManifestDescriptionDTO>();
            Mapper.CreateMap<ManifestDescriptionDTO, ManifestDescriptionCM>();

            Mapper.CreateMap<TerminalDO, TerminalDTO>()
                .ForMember(x => x.InternalId, x=>x.ResolveUsing(y => y.Id));
            Mapper.CreateMap<TerminalDO, TerminalSummaryDTO>();
            Mapper.CreateMap<TerminalDTO, TerminalDO>()
                .ForMember(x => x.LastUpdated, opts => opts.Ignore())
                .ForMember(x => x.CreateDate, opts => opts.Ignore())
                .ForMember(x => x.Id, opts => opts.Ignore())
                .ForMember(x => x.Secret, opts => opts.Ignore())
                .ForMember(x => x.AuthenticationTypeTemplate, opts => opts.Ignore());
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