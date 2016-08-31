using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.ViewModels;
using Fr8.Infrastructure.Data.States;

namespace HubWeb.App_Start
{
    public class AutoMapperBootStrapper
    {
        private readonly ITerminal _terminal;
        private readonly IActivityTemplate _activityTemplate;

        public AutoMapperBootStrapper(ITerminal terminal, IActivityTemplate activityTemplate)
        {
            _terminal = terminal;
            _activityTemplate = activityTemplate;
        }
        

        public void ConfigureAutoMapper()
        {
            Mapper.CreateMap<PlanDO, PlanNoChildrenDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Category, opts => opts.ResolveUsing(ad => ad.Category))
                .ForMember(a => a.Description, opts => opts.ResolveUsing(ad => ad.Description))
                .ForMember(a => a.LastUpdated, opts => opts.ResolveUsing(ad => ad.LastUpdated))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.PlanState, opts => opts.ResolveUsing(ad => PlanState.IntToString(ad.PlanState)))
                .ForMember(a => a.StartingSubPlanId, opts => opts.ResolveUsing(ad => ad.StartingSubPlanId))
                .ForMember(a => a.Tag, opts => opts.ResolveUsing(ad => ad.Tag))
                .ForMember(a => a.Visibility, opts => opts.ResolveUsing(ad => new PlanVisibilityDTO() { Hidden = ad.Visibility.BooleanValue() }));

            Mapper.CreateMap<PlanNoChildrenDTO, PlanDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Category, opts => opts.ResolveUsing(ad => ad.Category))
                .ForMember(a => a.Description, opts => opts.ResolveUsing(ad => ad.Description))
                .ForMember(a => a.LastUpdated, opts => opts.ResolveUsing(ad => ad.LastUpdated))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.PlanState, opts => opts.ResolveUsing(ad => PlanState.StringToInt(ad.PlanState)))
                .ForMember(a => a.StartingSubPlanId, opts => opts.ResolveUsing(ad => ad.StartingSubPlanId))
                .ForMember(a => a.Tag, opts => opts.ResolveUsing(ad => ad.Tag))
                .ForMember(a => a.Visibility, opts => opts.ResolveUsing(ad => ad.Visibility?.PlanVisibilityValue()));

            Mapper.CreateMap<ActivityDO, ActivityDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.RootPlanNodeId, opts => opts.ResolveUsing(ad => ad.RootPlanNodeId))
                .ForMember(a => a.ParentPlanNodeId, opts => opts.ResolveUsing(ad => ad.ParentPlanNodeId))
                .ForMember(a => a.ChildrenActivities, opts => opts.ResolveUsing(ad => ad.ChildNodes.OfType<ActivityDO>().OrderBy(da => da.Ordering)))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(GetActivityTemplate))
                .ForMember(a => a.AuthToken, opts => opts.ResolveUsing(ad => ad.AuthorizationToken));

            Mapper.CreateMap<ActivityDTO, ActivityDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.RootPlanNodeId, opts => opts.ResolveUsing(ad => ad.RootPlanNodeId))
                .ForMember(a => a.ParentPlanNodeId, opts => opts.ResolveUsing(ad => ad.ParentPlanNodeId))
                //.ForMember(a => a.ActivityTemplate, opts => opts.Ignore())
                .ForMember(a => a.ActivityTemplateId, opts => opts.ResolveUsing(GetActivityTemplateId))
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.SerializeObject(ad.CrateStorage)))
                .ForMember(a => a.ChildNodes, opts => opts.ResolveUsing(ad => MapActivities(ad.ChildrenActivities)))
                .ForMember(a => a.AuthorizationTokenId, opts => opts.ResolveUsing(ad => ad.AuthToken != null && ad.AuthToken.Id != null ? new Guid(ad.AuthToken.Id) : (Guid?)null));

            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateSummaryDTO>()
               .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
               .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
               .ForMember(x => x.TerminalName, opts => opts.ResolveUsing(x => x.Terminal.Name))
               .ForMember(x => x.TerminalVersion, opts => opts.ResolveUsing(x => x.Terminal.Version));

            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateDTO>()
               .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
               .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
               .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
               .ForMember(x => x.Terminal, opts => opts.ResolveUsing(GetTerminal))
               .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication));

            Mapper.CreateMap<Fr8AccountDO, ManageUserVM>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(account => !string.IsNullOrEmpty(account.PasswordHash)))
                .ForMember(mu => mu.HasGoogleToken, opts => opts.Ignore())
                .ForMember(mu => mu.GoogleSpreadsheets, opts => opts.Ignore());


            Mapper.CreateMap<UserVM, EmailAddressDO>()
                .ForMember(userDO => userDO.Address, opts => opts.ResolveUsing(e => e.EmailAddress));





            Mapper.CreateMap<UserVM, Fr8AccountDO>()
                .ForMember(userDO => userDO.Id, opts => opts.ResolveUsing(e => e.Id))
                .ForMember(userDO => userDO.FirstName, opts => opts.ResolveUsing(e => e.FirstName))
                .ForMember(userDO => userDO.LastName, opts => opts.ResolveUsing(e => e.LastName))
                .ForMember(userDO => userDO.UserName, opts => opts.ResolveUsing(e => e.UserName))
                .ForMember(userDO => userDO.EmailAddress, opts => opts.ResolveUsing(e => new EmailAddressDO { Address = e.EmailAddress }))
                .ForMember(userDO => userDO.Roles, opts => opts.Ignore());

            Mapper.CreateMap<PageDefinitionDTO, PageDefinitionDO>()
                .ForMember(dest => dest.Url, opts => opts.Ignore())
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(
                    x => x.Tags.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()));

            Mapper.CreateMap<PageDefinitionDO, PageDefinitionDTO>()
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(x => string.Join(", ", x.Tags)));

        }

        private static List<PlanNodeDO> MapActivities(IEnumerable<ActivityDTO> activities)
        {
            var list = new List<PlanNodeDO>();

            if (activities != null)
            {
                foreach (var activityDto in activities)
                {
                    list.Add(Mapper.Map<ActivityDO>(activityDto));
                }
            }

            return list;
        }

        private TerminalDTO GetTerminal(ActivityTemplateDO t)
        {
            if (t?.Terminal == null)
            {
                return null;
            }

            return Mapper.Map<TerminalDTO>(t.Terminal);
        }

        private ActivityTemplateSummaryDTO GetActivityTemplate(ActivityDO ad)
        {
            if (ad.ActivityTemplate != null)
            {
                return Mapper.Map<ActivityTemplateSummaryDTO>(ad.ActivityTemplate);
            }

            if (ad.ActivityTemplateId == Guid.Empty)
            {
                return null;
            }

            return Mapper.Map<ActivityTemplateSummaryDTO>(_activityTemplate.GetByKey(ad.ActivityTemplateId));
        }

        private object GetActivityTemplateId(ActivityDTO ad)
        {
            if (ad.ActivityTemplate == null)
            {
                return null;
            }

            return _activityTemplate.GetByNameAndVersion(ad.ActivityTemplate).Id;
        }
    }
}
//Missing type map configuration or unsupported mapping.

//Mapping types:
//UserVM -> EmailAddressDO
//Web.ViewModels.UserVM -> Data.Entities.EmailAddressDO

//Destination path:
//DockyardAccountDO

//Source value:
//Web.ViewModels.UserVM