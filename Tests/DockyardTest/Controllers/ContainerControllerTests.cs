using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using StructureMap.AutoMocking;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Data.States;
using System.Threading.Tasks;


namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("ContainerControllerTests")]
    class ContainerControllerTests :BaseTest
    {
        private Fr8AccountDO _testUserAccount;

        private Core.Interfaces.IContainer _containerService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _testUserAccount = FixtureData.TestUser1();
            _containerService = ObjectFactory.GetInstance<Core.Interfaces.IContainer>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var envelopeCrate = FixtureData.EnvelopeIdCrateJson();
                var processTemplate = FixtureData.TestRouteWithStartingSubrouteAndActionList();

                uow.RouteRepository.Add(processTemplate);
                uow.UserRepository.Add(_testUserAccount);
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, _testUserAccount.Id);
                uow.SaveChanges();
                var container = _containerService.Create(uow, processTemplate.Id, FixtureData.GetEnvelopeIdCrate());
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetQuery()
                    .SingleOrDefault(x => x.Id == _testUserAccount.Id);

                uow.UserRepository.Remove(curUser);
                uow.SaveChanges();
            }
        }

        [Test]
        public void ContainerController_Will_ReturnEmptyOkResult_If_No_Container_Found()
        {
            //Act
            var containerController = CreateContainerController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
        
            int? id = 55;
            var actionResult = containerController.Get(id);
            //Assert
            Assert.IsNull(actionResult as OkNegotiatedContentResult<ContainerDO>);
        }

        [Test]
        public void ContainerController_Will_Return_All_UserContainers_When_Get_Invoked_With_Null()
        {
            //Arrange
            var containerController = CreateContainerController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            //Act
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                int? id = null;
                var actionResult = containerController.Get(id) as OkNegotiatedContentResult<IEnumerable<ContainerDTO>>;

                ////Assert
                Assert.NotNull(actionResult);
                // Assert.AreEqual(2, actionResult.Content.Count());
            }
        }


        [Test]
        public void ContainerController_Will_Return_Single_Container_When_Get_Invoked_With_Id()
        {
            //Arrange
            var containerController = CreateContainerController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            //Act
            int? id = 1;
            var actionResult = containerController.Get(id) as OkNegotiatedContentResult<ContainerDTO>;

            ////Assert
            Assert.NotNull(actionResult);
            
        }

        private static ContainerController CreateContainerController(string userId, string email)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Name, email));
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Role,Roles.Admin));

            var identity = new ClaimsIdentity(claims);
            
            var containerController = new ContainerController
            {
                User = new GenericPrincipal(identity, new[] { "Users" })
            };

            return containerController;
        }
      
    }
}
