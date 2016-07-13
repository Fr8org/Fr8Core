using System.Linq;
using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using StructureMap;
using terminalAsana.Interfaces;
using terminalAsana.Asana;
using terminalAsana.Asana.Entities;
using terminalAsana.Asana.Services;

namespace terminalAsana
{
    public static class TerminalAsanaBootstrapper
    {
        public static void ConfigureAsanaDependencies(this IContainer container)
        {
            container.Configure(ConfigureLive);
        }

        /**********************************************************************************/

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            configurationExpression.For<IAsanaParameters>().Use<AsanaParametersService>().Singleton();
            configurationExpression.For<IAsanaOAuth>().Use<AsanaOAuthService>();
            configurationExpression.For<IAsanaWorkspaces>().Use<Workspaces>();
            configurationExpression.For<IAsanaUsers>().Use<Users>();

            TerminalAsanaBootstrapper.ConfigureAutoMappert();
        }

        public static void ConfigureAutoMappert()
        {
            Mapper.CreateMap<AuthorizationToken, AuthorizationTokenDTO>();

            Mapper.CreateMap<AsanaTask, AsanaTaskCM>()
                .ForMember(a => a.Assignee, opt => opt.ResolveUsing(cm => cm.Assignee.Id))
                .ForMember(a => a.Followers, opt => opt.ResolveUsing(cm => cm.Followers.Select(f => f.Id)))
                .ForMember(a => a.Parent, opt => opt.ResolveUsing(cm => cm.Parent.Id))
                .ForMember(a => a.Hearts, opt => opt.ResolveUsing(cm => cm.Hearts.Select(h => h.Id)))
                .ForMember(a => a.Projects, opt => opt.ResolveUsing(cm => cm.Projects.Select(p => p.Id)))
                .ForMember(a => a.Tags, opt => opt.ResolveUsing(cm => cm.Tags.Select(t => t.Id)))
                .ForMember(a => a.Workspace, opt => opt.ResolveUsing(cm => cm.Workspace.Id));
        }
    }
}