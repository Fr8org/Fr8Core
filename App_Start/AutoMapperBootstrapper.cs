using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.ViewModels;

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
            Mapper.CreateMap<ActivityDO, ActivityDTO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.RootRouteNodeId, opts => opts.ResolveUsing(ad => ad.RootRouteNodeId))
                .ForMember(a => a.ParentRouteNodeId, opts => opts.ResolveUsing(ad => ad.ParentRouteNodeId))
                .ForMember(a => a.ActivityTemplateId, opts => opts.ResolveUsing(ad => ad.ActivityTemplateId))
                .ForMember(a => a.CurrentView, opts => opts.ResolveUsing(ad => ad.currentView))
                .ForMember(a => a.ChildrenActions, opts => opts.ResolveUsing(ad => ad.ChildNodes.OfType<ActivityDO>().OrderBy(da => da.Ordering)))
                .ForMember(a => a.ActivityTemplate, opts => opts.ResolveUsing(GetActivityTemplate))
                .ForMember(a => a.AuthToken, opts => opts.ResolveUsing(ad => ad.AuthorizationToken));

            Mapper.CreateMap<ActivityDTO, ActivityDO>().ForMember(a => a.Id, opts => opts.ResolveUsing(ad => ad.Id))
                .ForMember(a => a.Name, opts => opts.ResolveUsing(ad => ad.Name))
                .ForMember(a => a.RootRouteNodeId, opts => opts.ResolveUsing(ad => ad.RootRouteNodeId))
                .ForMember(a => a.ParentRouteNodeId, opts => opts.ResolveUsing(ad => ad.ParentRouteNodeId))
                .ForMember(a => a.ActivityTemplateId, opts => opts.ResolveUsing(ad => ad.ActivityTemplateId))
                .ForMember(a => a.ActivityTemplate, opts => opts.Ignore())
                //.ForMember(a => a.CrateStorage, opts => opts.ResolveUsing(ad => Newtonsoft.Json.JsonConvert.SerializeObject(ad.CrateStorage)))
                .ForMember(a => a.currentView, opts => opts.ResolveUsing(ad => ad.CurrentView))
                .ForMember(a => a.ChildNodes, opts => opts.ResolveUsing(ad => MapActions(ad.ChildrenActions)))
                .ForMember(a => a.IsTempId, opts => opts.ResolveUsing(ad => ad.IsTempId))
                .ForMember(a => a.AuthorizationTokenId, opts => opts.ResolveUsing(ad => ad.AuthToken != null && ad.AuthToken.Id != null ? new Guid(ad.AuthToken.Id) : (Guid?)null));


            Mapper.CreateMap<ActivityTemplateDO, ActivityTemplateDTO>()
               .ForMember(x => x.Id, opts => opts.ResolveUsing(x => x.Id))
               .ForMember(x => x.Name, opts => opts.ResolveUsing(x => x.Name))
               .ForMember(x => x.Version, opts => opts.ResolveUsing(x => x.Version))
               .ForMember(x => x.Description, opts => opts.ResolveUsing(x => x.Description))
               .ForMember(x => x.TerminalId, opts => opts.ResolveUsing(x => x.TerminalId))
               .ForMember(x => x.Terminal, opts => opts.ResolveUsing(GetTerminal))
               .ForMember(x => x.NeedsAuthentication, opts => opts.ResolveUsing(x => x.NeedsAuthentication));

            Mapper.CreateMap<Fr8AccountDO, ManageUserVM>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(account => !string.IsNullOrEmpty(account.PasswordHash)))
                .ForMember(mu => mu.HasDocusignToken, opts => opts.Ignore())
                .ForMember(mu => mu.HasGoogleToken, opts => opts.Ignore())
                .ForMember(mu => mu.GoogleSpreadsheets, opts => opts.Ignore());

          
            Mapper.CreateMap<UserVM, EmailAddressDO>()
                .ForMember(userDO => userDO.Address, opts => opts.ResolveUsing(e => e.EmailAddress));

            Mapper.CreateMap<UserVM, Fr8AccountDO>()
                .ForMember(userDO => userDO.Id, opts => opts.ResolveUsing(e => e.Id))
                .ForMember(userDO => userDO.FirstName, opts => opts.ResolveUsing(e => e.FirstName))
                .ForMember(userDO => userDO.LastName, opts => opts.ResolveUsing(e => e.LastName))
                .ForMember(userDO => userDO.UserName, opts => opts.ResolveUsing(e => e.UserName))
                .ForMember(userDO => userDO.EmailAddress, opts => opts.ResolveUsing(e => new EmailAddressDO {Address = e.EmailAddress}))
                .ForMember(userDO => userDO.Roles, opts => opts.Ignore());
        }

        private static List<RouteNodeDO> MapActions(IEnumerable<ActivityDTO> activities)
        {
            var list = new List<RouteNodeDO>();

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
            if (t == null)
            {
                return null;
            }

            return Mapper.Map<TerminalDTO>(_terminal.GetByKey(t.TerminalId));
        }

        private ActivityTemplateDTO GetActivityTemplate(ActivityDO ad)
        {
            if (ad.ActivityTemplateId == null)
            {
                return null;
            }

            return Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByKey(ad.ActivityTemplateId.Value));
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