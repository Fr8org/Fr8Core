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
using Web;
using Data.Interfaces.DataTransferObjects;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("StartupTests")]
    public class StartupTests : BaseTest
    {
        IUnitOfWork _uow;
        public override void SetUp()
        {
            base.SetUp();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
        }

        [Test]
        public void RegisterPluginActions_InheritBaseRegistration_RegisterNewAction()
        {
            Startup startup = new Startup();
            startup.RegisterPluginActions();

            var recordCount = _uow.ActionTemplateRepository.GetQuery().Count();

            Assert.GreaterOrEqual(recordCount, 1);
        }
    }

    public class TestPluginRegistration : BasePluginRegistration
    {
        public const string baseUrl = "AzureSql.BaseUrl";
        private const string availableActions = @"[{ ""ActionType"" : ""Write"" , ""Version"": ""1.0""}]";
        public const string PluginRegistrationName = "Test";

        public TestPluginRegistration()
            : base(InitAvailableActions(), baseUrl, PluginRegistrationName)
        {

        }

        private static ActionNameListDTO InitAvailableActions()
        {
            ActionNameListDTO curActionNameList = new ActionNameListDTO();
            curActionNameList.ActionNames.Add(new ActionNameDTO
            {
                Name = "Write",
                Version = "1.0"
            });
            return curActionNameList;
        }


    }
}
