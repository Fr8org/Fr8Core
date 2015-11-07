using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using Hub.Interfaces;
using Hub.Managers;
using Hub.StructureMap;
using Moq;
using NUnit.Framework;
using SendGrid;
using StructureMap;
using terminalSendGrid.Actions;
using terminalSendGrid.Infrastructure;
using terminalSendGrid.Services;
using terminalSendGrid.Tests.Fixtures;
using Utilities;

namespace terminalSendGrid.Tests.Actions
{
    [TestFixture]
    [Category("terminalSendGrid")]
    public class SendEmailViaSendGrid_v1Tests : BaseTest
    {
        private SendEmailViaSendGrid_v1 _gridAction;
        private ICrateManager _crate;

        public override void SetUp()
        {
            base.SetUp();

            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.TEST;

            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).SendGridConfigureDependencies(dependencyType);
            ObjectFactory.Configure(cfg => cfg.For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>())));
            ObjectFactory.Configure(cfg => cfg.For<IEmailPackager>().Use(new SendGridPackager()));

            _crate = ObjectFactory.GetInstance<ICrateManager>();

            var routeNode = new Mock<IRouteNode>();
            routeNode.Setup(c => c.GetCratesByDirection(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<GetCrateDirection>()))
                    .Returns(Task.FromResult(new List<Crate>()));

            ObjectFactory.Configure(cfg => cfg.For<IRouteNode>().Use(routeNode.Object));

            var routeNodeeneric = new Mock<IRouteNode>();
            routeNodeeneric.Setup(c => c.GetCratesByDirection<StandardDesignTimeFieldsCM>(It.IsAny<int>(), It.IsAny<GetCrateDirection>()))
                    .Returns(Task.FromResult(new List<Crate<StandardDesignTimeFieldsCM>>()));

            ObjectFactory.Configure(cfg => cfg.For<IRouteNode>().Use(routeNodeeneric.Object));
        }

        [Test]
        public void Configure_ReturnsCrateDTO()
        {
            // Act
            var actionResult = GetActionResult();
            var controlsCrates = actionResult.CrateStorage.Crates;

            // Assert
            Assert.IsNotNull(controlsCrates);
            Assert.AreEqual(controlsCrates.Count(), 2);
        }

        [Test]
        [Ignore]
        public void Configure_ReturnsCrateDTOStandardConfigurationControls()
        {
            // Act
            var actionResult = GetActionResult();
            var controlsCrates = actionResult.CrateStorage.Crates.FirstOrDefault();
            //var standardControls = _crate.GetStandardConfigurationControls(controlsCrates);

            //// Assert
            //Assert.IsNotNull(standardControls);
            //Assert.IsNotNull(standardControls.Controls);
            //Assert.AreEqual(standardControls.Controls.Count, 3);
        }

        private ActionDTO GetActionResult()
        {
            _gridAction = new SendEmailViaSendGrid_v1();
            var action = FixtureData.ConfigureSendEmailViaSendGridAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);
            
            var actionResult = _gridAction.Configure(curActionDTO).Result;
            return actionResult;
        }

        [Test]
        [Ignore("Moq cannot mock non virtual methods on classes. Cannot mock HttpClient")]
        public void Run_Returns_PayloadDTO()
        {
            // Arrange
            _gridAction = new SendEmailViaSendGrid_v1();
            var action = FixtureData.ConfigureSendEmailViaSendGridAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);

            //// Act
            //var payloadDTOResult = _gridAction.Run(curActionDTO).Result;

            //// Assert
            //Assert.NotNull(payloadDTOResult);
        }
    }
}
