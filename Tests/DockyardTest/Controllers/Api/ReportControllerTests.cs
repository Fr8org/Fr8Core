using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories;
using Data.States;
using HubWeb.Controllers;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting.Fixtures;


namespace DockyardTest.Controllers.Api
{
    [TestFixture]
    [Category("ReportControllerTests")]
    class ReportControllerTests : ApiControllerTestBase
    {
        private Fr8AccountDO _testUserAccount;
        private Hub.Interfaces.IReport _report;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _testUserAccount = FixtureData.TestDockyardAccount7();
            _report = ObjectFactory.GetInstance<Hub.Interfaces.IReport>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.UserRepository.Add(_testUserAccount);
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, _testUserAccount.Id);
                uow.SaveChanges();
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, _testUserAccount);
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
        public void ReportController_Returns_Two_Incidents()
        {
            //Arrange
            var reportController = CreateReportController();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AddIncidents(uow);

                //Act
                var actionResult =
                    reportController.GetIncidents(1, 2, "all") as OkNegotiatedContentResult<List<HistoryItemDTO>>;

                //Assert
                Assert.NotNull(actionResult);
                Assert.AreEqual(2, actionResult.Content.ToList().Count());
            }
        }
        [Test]
        public void ReportController_Returns_Ten_Incidents()
        {
            //Arrange
            var reportController = CreateReportController();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AddIncidents(uow);

                //Act
                var actionResult =
                    reportController.GetIncidents(1, 10, "all") as OkNegotiatedContentResult<List<HistoryItemDTO>>;

                //Assert
                Assert.NotNull(actionResult);
                Assert.AreEqual(10, actionResult.Content.ToList().Count());
            }
        }
        [Test]
        public void ReportController_Returns_SecondPage_CorrectIncidents()
        {
            //Arrange
            var reportController = CreateReportController();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AddIncidents(uow);

                //Act
                var actionResult =
                    reportController.GetIncidents(3, 3, "all") as OkNegotiatedContentResult<List<HistoryItemDTO>>;

                //Assert
                Assert.NotNull(actionResult);
                Assert.AreEqual(3, actionResult.Content.ToList().Count());
                Assert.AreEqual("Incident 3", actionResult.Content.ToList()[0].PrimaryCategory);
            }
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReportController_IncorrectUserValue_ThrowsException()
        {
            //Arrange
            var reportController = CreateReportController();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Act
                var actionResult =
                    reportController.GetIncidents(1, 2, "IncorrectUserParameter") as OkNegotiatedContentResult<List<HistoryItemDTO>>;
            }
        }

        private void AddIncidents(IUnitOfWork uow)
        {
            //Arrange 
            using (uow)
            {
                foreach (var incident in FixtureData.TestIncidentsForReportControllerTest())
                    uow.IncidentRepository.Add(incident);
                uow.SaveChanges();
            }
        }
        private static ReportController CreateReportController()
        {
            return new ReportController();
        }
    }
}
