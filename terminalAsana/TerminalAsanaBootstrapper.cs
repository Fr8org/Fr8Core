using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;
using StructureMap;
using terminalAsana.Interfaces;
using terminalAsana.Asana;

namespace terminalAsana
{
    public static class TerminalAsanaBootstrapper
    {
        public static void ConfigureAsanaDependencies(this IContainer container)
        {
            /*
            switch (type)
            {
                case StructureMapBootStrapper.TEST:
                    container.Configure(ConfigureLive); // no test mode yet
                    break;

                case StructureMapBootStrapper.DependencyType.LIVE:
                    container.Configure(ConfigureLive);
                    break;
            }*/
            container.Configure(ConfigureLive);
        }

        /**********************************************************************************/

        public static void ConfigureLive(ConfigurationExpression configurationExpression)
        {
            //configurationExpression.For<IAsanaCommunication>().Use<AsanaCommunication>().Singleton();
            configurationExpression.For<IAsanaOAuth>().Use<AsanaOAuthService>().Singleton();
            configurationExpression.For<IAsanaUsers>().Use<Users>();

            TerminalAsanaBootstrapper.ConfigureAutoMappert();
        }

        public static void ConfigureAutoMappert()
        {
            Mapper.CreateMap<AuthorizationToken, AuthorizationTokenDTO>();
        }
    }
}