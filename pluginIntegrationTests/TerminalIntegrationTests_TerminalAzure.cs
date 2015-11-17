using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;

namespace terminalIntegrationTests
{
    public partial class TerminalIntegrationTests
    {
        [Test]
        public async void TerminalAzure_DiscoverTerminals_ShouldReturnDataInCorrectFormat()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var route = new RouteDO()
                {
                    Id = 1,
                    Name = "1",
                    RouteState = RouteState.Active
                };

                uow.RouteRepository.Add(route);

                uow.ContainerRepository.Add(new ContainerDO()
                {
                    Id = UtilitiesTesting.Fixtures.FixtureData.TestContainer_Id_1(),
                    Route = route,
                    CrateStorage = _crateManager.EmptyStorageAsStr(),
                    ContainerState = ContainerState.Executing
                });

                uow.SaveChanges();
            }
        }
    }
}
