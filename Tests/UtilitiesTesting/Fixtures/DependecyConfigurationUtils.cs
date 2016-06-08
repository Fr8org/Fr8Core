using System;
using System.Threading.Tasks;
using Data.Entities;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using Moq;
using StructureMap;
using TerminalBase.Infrastructure;

namespace UtilitiesTesting.Fixtures
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
