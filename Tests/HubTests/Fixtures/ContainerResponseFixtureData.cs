using Data.Entities;
using Hub.Services;

namespace HubTests.Fixtures
{
    public partial class FixtureData
    {
        public static Container EmptyContainer()
        {
            return new Container();
        }

        public static ContainerDO EmptyContainerDO()
        {
            return new ContainerDO();
        }
    }
}
