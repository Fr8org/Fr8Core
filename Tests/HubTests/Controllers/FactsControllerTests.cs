using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using HubTests.Controllers.Api;
using HubWeb.Controllers.Api;
using NUnit.Framework;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using UtilitiesTesting.Fixtures;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("FactsControllerTests")]
    class FactsControllerTests : ApiControllerTestBase
    {
        private Fr8AccountDO _testUserAccount;
        private Hub.Interfaces.IFact _factService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _testUserAccount = FixtureData.TestDockyardAccount5();
            _factService = ObjectFactory.GetInstance<Hub.Interfaces.IFact>();

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                unitOfWork.UserRepository.Add(_testUserAccount);
                var plan = FixtureData.TestPlan4();
                var container = FixtureData.TestContainerForFactsControllerTest();
                unitOfWork.PlanRepository.Add(plan);
                unitOfWork.ContainerRepository.Add(container);
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
            var factsController = CreateFactsController();
            AddFacts();

            //Act
            ContainerDO container = FixtureData.TestContainerForFactsControllerTest();
            FactDO query = new FactDO()
            {
                ObjectId = container.Id.ToString()
            };
            var actionResult = factsController.ProcessQuery(query) as OkNegotiatedContentResult<IEnumerable<FactDTO>>;

            ////Assert
            Assert.NotNull(actionResult);
            Assert.AreEqual(2, actionResult.Content.Count());
        }

        private void AddFacts()
        {
            //Arrange 
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var fact in FixtureData.TestFactsForFactsControllerTest())
                {
                    unitOfWork.FactRepository.Add(fact);
                }
                unitOfWork.SaveChanges();
            }
        }

        private static FactsController CreateFactsController()
        {
            return new FactsController();
        }
    }
}
