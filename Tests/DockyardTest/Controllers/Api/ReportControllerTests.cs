using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.Controllers;
using HubWeb.Controllers.Api;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Controllers.Api
{
    [TestFixture]
    [Category("ReportControllerTests")]
    public class ReportControllerTests : ApiControllerTestBase
    {
        private Fr8AccountDO _testUserAccount;
        private IReport _report;
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _testUserAccount = FixtureData.TestDockyardAccount7();
            _report = ObjectFactory.GetInstance<IReport>();

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                unitOfWork.UserRepository.Add(_testUserAccount);
                unitOfWork.SaveChanges();
                ObjectFactory.GetInstance<ISecurityServices>().Login(unitOfWork, _testUserAccount);
            }
        }
        [TearDown]
        public void TearDown()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetQuery()
                    .SingleOrDefault(x => x.Id == _testUserAccount.Id);

                ObjectFactory.GetInstance<ISecurityServices>().Logout();

                uow.UserRepository.Remove(curUser);
                uow.SaveChanges();
            }
        }

        [Test]
        public void FactsController_Will_Return_All_Facts_For_Given_ObjectId()
        {
            //Arrange
            var reportController = CreateReportController();
            AddIncidents();

            //Act
            var actionResult = reportController.GetTopIncidents(1,2,"all") as OkNegotiatedContentResult<IEnumerable<IncidentDTO>>;

            ////Assert
            Assert.NotNull(actionResult);
            Assert.AreEqual(2, actionResult.Content.Count());
        }

        private void AddIncidents()
        {
            //Arrange 
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var incident in FixtureData.TestIncidentsForReportControllerTest())
                {
                    unitOfWork.IncidentRepository.Add(incident);
                }
                unitOfWork.SaveChanges();
            }
        }
        private static ReportController CreateReportController()
        {
            return new ReportController();
        }
    }
}
