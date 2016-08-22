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
                .ForMember(cm=>cm.Assignee, opt => opt.ResolveUsing(at => at.Assignee?.Id))
                .ForMember(cm=>cm.Followers, opt => opt.ResolveUsing(at => at.Followers?.Select(f => f.Id)))
                .ForMember(cm=>cm.Parent, opt => opt.ResolveUsing(at => at.Parent?.Id))
                .ForMember(cm=>cm.Hearts, opt => opt.ResolveUsing(at => at.Hearts?.Select(h => h.Id)))
                .ForMember(cm=>cm.Projects, opt => opt.ResolveUsing(at => at.Projects?.Select(p => p.Id)))
                .ForMember(cm=>cm.Tags, opt => opt.ResolveUsing(at => at.Tags?.Select(t => t.Id)))
                .ForMember(cm=>cm.Workspace, opt => opt.ResolveUsing(at => at.Workspace?.Id));
        }
    }
}