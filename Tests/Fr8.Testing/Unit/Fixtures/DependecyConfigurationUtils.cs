using System;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.Testing.Unit.Fixtures
{
    public static class DependecyConfigurationUtils
    {
        public static IContainer ConfigureHubToReturnEmptyPayload(this IContainer container)
        {
            var payload = new PayloadDTO(Guid.NewGuid());
            using (var storage = container.GetInstance<ICrateManager>().GetUpdatableStorage(payload))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            }
            container.GetInstance<Mock<IHubCommunicator>>().Setup(x => x.GetPayload(It.IsAny<Guid>()))
                     .Returns(Task.FromResult(payload));
            return container;
        }
    }
}
