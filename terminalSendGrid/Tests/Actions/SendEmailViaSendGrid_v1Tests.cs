using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
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
            
            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>()))
                .Returns(Task.FromResult(FixtureData.CratePayloadDTOForSendEmailViaSendGridConfiguration));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));

            _crate = ObjectFactory.GetInstance<ICrateManager>();

            var routeNode = new Mock<IRouteNode>();
            routeNode.Setup(c => c.GetCratesByDirection<StandardDesignTimeFieldsCM>(It.IsAny<int>(), It.IsAny<GetCrateDirection>()))
                    .Returns(Task.FromResult(new List<Crate<StandardDesignTimeFieldsCM>>()));

            ObjectFactory.Configure(cfg => cfg.For<IRouteNode>().Use(routeNode.Object));
        }

        [Test]
        public void Configure_ReturnsCrateDTO()
        {
            // Arrange
            var actionResult = GetActionResult();

            // Act
            var controlsCrates = actionResult.CrateStorage.Crates;

            // Assert
            Assert.IsNotNull(controlsCrates);
            Assert.AreEqual(controlsCrates.Count(), 2);
        }

        [Test]
        public void Configure_ReturnsCrateDTOStandardConfigurationControlsMS()
        {
            // Arrange
            var actionResult = GetActionResult();

            // Act
            var controlsCrate = _crate.FromDto(actionResult.CrateStorage).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            // Assert
            Assert.IsNotNull(controlsCrate);
            Assert.IsNotNull(controlsCrate.ManifestType);
            Assert.IsNotNull(controlsCrate.Content);
            Assert.AreEqual(controlsCrate.Content.Controls.Count, 3);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Configure_ReturnsEmailControls(int index)
        {
            // Arrange
            var actionResult = GetActionResult();

            // Act && Assert
            var standardControls = _crate.FromDto(actionResult.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            Assert.IsNotNull(standardControls);

            var specificValueTextField = ((RadioButtonGroupControlDefinitionDTO)standardControls.Controls[index]).Radios.SelectMany(c => c.Controls).Count(s => s.Name == "SpecificValue");
            Assert.AreEqual(specificValueTextField, 1);

            var upstreamCrateField = ((RadioButtonGroupControlDefinitionDTO)standardControls.Controls[index]).Radios.SelectMany(c => c.Controls).Count(s => s.Name == "UpstreamCrate");
            Assert.AreEqual(upstreamCrateField, 1);
        }

        private ActionDTO GetActionResult()
        {
            _gridAction = new SendEmailViaSendGrid_v1();
            var curActionDO = FixtureData.ConfigureSendEmailViaSendGridAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(curActionDO);
            var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(curActionDTO.AuthToken);
            var actionResult = _gridAction.Configure(curActionDO, curAuthTokenDO).Result;
            return Mapper.Map<ActionDTO>(actionResult);
        }

        [Test]
        public void Run_Returns_PayloadDTO()
        {
            // Arrange
            var actionResult = GetActionResult();

            _gridAction = new SendEmailViaSendGrid_v1();
            var curActionDO = FixtureData.ConfigureSendEmailViaSendGridAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(curActionDO);
            curActionDTO.CrateStorage = actionResult.CrateStorage;
            var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(curActionDTO.AuthToken);
            // Act
            var payloadDTOResult = _gridAction.Run(curActionDO, curActionDTO.ContainerId, curAuthTokenDO).Result;

            // Assert
            Assert.NotNull(payloadDTOResult);
        }
    }
}
