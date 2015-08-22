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
using Data.Interfaces.DataTransferObjects;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("BasePluginRegistrationService")]
    public class BasePluginRegistrationTests : BaseTest
    {
        string actionType = "Write SQL";
        string version = "1.0";
        //string availableActions;
        string baseUrl = "AzureSql.BaseUrl";
        IUnitOfWork _uow;
        ActionNameListDTO curActionNameList;
        public override void SetUp()
        {
 	         base.SetUp();
             _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            // availableActions = @"[{ ""ActionType"" : """+ actionType + @""" , ""Version"": """ + version + @"""}]";
             curActionNameList = FixtureData.TestActionNameListDTO1();
        }

        [Test]
        public void AvailableCommands_CorrectFormat_ReturnsCorrectCount()
        {
            //availableActions = @"[{ ""ActionType"" : ""Write"" , ""Version"": ""1.0""}, { ""ActionType"" : ""Write"" , ""Version"": ""1.4""}]";
            

            var basePluginRegistrationMock = new Mock<BasePluginRegistration>(curActionNameList, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));

            var result = ObjectFactory.GetInstance<BasePluginRegistration>().AvailableActions;

            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void AvailableCommands_CorrectFormat_ReturnsCorrectLists()
        {
            var basePluginRegistrationMock = new Mock<BasePluginRegistration>(curActionNameList, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));

            var result = ObjectFactory.GetInstance<BasePluginRegistration>().AvailableActions;

            //Assert.AreEqual(actionType, result.ToList()[0].ActionType);
            //Assert.AreEqual(version, result.ToList()[0].Version);

            Assert.AreEqual(actionType, ((List<ActionRegistrationDO>)result)[0].ActionType);
            Assert.AreEqual(version, ((List<ActionRegistrationDO>)result)[0].Version);
        }

        [Test]
        public void RegisterActions_RegisterNew_CreatesActionRegistrationDO()
        {
            var basePluginRegistrationMock = new Mock<BasePluginRegistration>(curActionNameList, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));

            var basePluginRegistration = ObjectFactory.GetInstance<BasePluginRegistration>();
            basePluginRegistration.RegisterActions();

            var newActionRegistration = _uow.ActionRegistrationRepository.GetQuery()
                .FirstOrDefault(i => i.ActionType == actionType && i.Version == version);

            Assert.AreEqual(actionType, newActionRegistration.ActionType);
            Assert.AreEqual(version, newActionRegistration.Version);
        }

        [Test]
        public void RegisterActions_RegisterExisting_DoNoCreateNew()
        {
            var basePluginRegistrationMock = new Mock<BasePluginRegistration>(curActionNameList, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));

            var basePluginRegistration = ObjectFactory.GetInstance<BasePluginRegistration>();
            basePluginRegistration.RegisterActions();
            int totalRecords = _uow.ActionRegistrationRepository.GetQuery().Count();

            basePluginRegistrationMock = new Mock<BasePluginRegistration>(curActionNameList, baseUrl);
            ObjectFactory.Configure(cfg => cfg.For<BasePluginRegistration>().Use(basePluginRegistrationMock.Object));
            basePluginRegistration.RegisterActions();
            int totalRecordsAfterExistingRegister = _uow.ActionRegistrationRepository.GetQuery().Count();

            Assert.AreEqual(totalRecords, totalRecordsAfterExistingRegister);
        }
    }
}
