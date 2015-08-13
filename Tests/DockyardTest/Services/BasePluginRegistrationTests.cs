using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.PluginRegistrations;
using Data.Interfaces;
using Moq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Entities;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("BasePluginRegistrationService")]
    public class BasePluginRegistrationTests : BaseTest
    {
        string actionType = "Write SQL";
        string version = "1.0";
        string availableActions;
        string baseUrl = "AzureSql.BaseUrl";
        IUnitOfWork _uow;

        public override void SetUp()
        {
 	         base.SetUp();
             _uow = ObjectFactory.GetInstance<IUnitOfWork>();
             availableActions = @"[{ ""ActionType"" : """+ actionType + @""" , ""Version"": """ + version + @"""}]";
        }

        [Test]
        public void AvailableCommands_CorrectFormat_ReturnsCorrectCount()
        {
            availableActions = @"[{ ""ActionType"" : ""Write"" , ""Version"": ""1.0""}, { ""ActionType"" : ""Write"" , ""Version"": ""1.4""}]";
            var basePluginRegistrationMock = new Mock<BasePluginRegistration>(availableActions, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));

            var result = ObjectFactory.GetInstance<BasePluginRegistration>().AvailableCommands;

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void AvailableCommands_CorrectFormat_ReturnsCorrectLists()
        {
            var basePluginRegistrationMock = new Mock<BasePluginRegistration>(availableActions, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));

            var result = ObjectFactory.GetInstance<BasePluginRegistration>().AvailableCommands;

            Assert.AreEqual(actionType, result.ToList()[0].ActionType);
            Assert.AreEqual(version, result.ToList()[0].Version);
        }

        [Test]
        public void RegisterActions_RegisterNew_CreatesActionRegistrationDO()
        {
            var basePluginRegistrationMock = new Mock<BasePluginRegistration>(availableActions, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));

            var basePluginRegistration = ObjectFactory.GetInstance<BasePluginRegistration>();
            basePluginRegistration.RegisterActions();

            var newActionRegistration = _uow.ActionRegistrationRepository.GetQuery()
                .FirstOrDefault(i => i.ActionType == actionType && i.Version == version);

            Assert.AreEqual(actionType, newActionRegistration.ActionType);
            Assert.AreEqual(version, newActionRegistration.Version);
        }
    }
}
