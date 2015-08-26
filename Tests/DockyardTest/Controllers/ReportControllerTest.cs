using System.Linq;
using System.Web.Http.Results;
using Core.Managers;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Data.Entities;
using System.Collections.Generic;
using Utilities;
using Newtonsoft.Json;


namespace DockyardTest.Controllers
{
    [TestFixture]
    public class ReportControllerTest : BaseTest
    {
        private ReportController _reportController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _reportController = new ReportController();
        }

        [Test]
        [Category("Controllers.ReportController.ShowReport")]
        public void ReportController_ShowReport_ReturnsJsonResult()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curFactDO = FixtureData.TestFactDO();

                uow.FactRepository.Add(curFactDO);
                uow.SaveChanges();

                //Act
                var result = _reportController.ShowReport("all", "usage", 1, 0, 10);

                //Cast anonymous object into dictionary object
                var dictionaryResult = result.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(result, null));

                // Get json data 
                var jsonData = dictionaryResult["Data"];
                int recordCount = (int)jsonData.GetType().GetProperty("recordsTotal").GetValue(jsonData);

                //Assert
                Assert.AreEqual(1, recordCount);
            }

        }
    }
}
