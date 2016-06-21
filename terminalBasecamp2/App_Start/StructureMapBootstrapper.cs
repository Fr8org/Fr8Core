using StructureMap;
using terminalBasecamp.Infrastructure;

namespace terminalBasecamp.App_Start
{
    public class StructureMapBootstrapper
    {
        public static void LiveMode(ConfigurationExpression expression)
        {
            expression.For<IBasecampAuthorization>().Use<BasecampAuthorization>().Singleton();
            expression.For<IBasecampApi>().Use<BasecampApi>().Singleton();
        }
    }
}