using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Core.Services;
using Web.Controllers;
using Web.ViewModels;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using Data.Interfaces.DataTransferObjects;
using DockyardTest.Controllers.Api;
using UtilitiesTesting.Fixtures;
using System.Web.Http.Results;

namespace DockyardTest.Controllers
{
    public class UserControllerTests : ApiControllerTestBase
    {
        private DockyardAccountDO _testAccount1;
        private DockyardAccountDO _testAccount2;
        private DockyardAccountDO _testAccount3;

        public override void SetUp()
        {
            base.SetUp();
            InitializeUsers();
        }

        [Test]
        public void Get()
        {
            var controller = CreateController<UserController>();

            var result = controller.Get() as OkNegotiatedContentResult<List<UserDTO>>;
                
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);

            Assert.AreEqual(result.Content.Count, 3);
            Assert.AreEqual(result.Content[0].Id, _testAccount1.Id);
            Assert.AreEqual(result.Content[1].Id, _testAccount2.Id);
            Assert.AreEqual(result.Content[2].Id, _testAccount3.Id);
        }

        private void InitializeUsers()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _testAccount1 = FixtureData.TestUser1();
                uow.UserRepository.Add(_testAccount1);

                _testAccount2 = FixtureData.TestUser2();
                uow.UserRepository.Add(_testAccount2);

                _testAccount3 = FixtureData.TestUser3();
                uow.UserRepository.Add(_testAccount3);

                uow.SaveChanges();
            }
        }
    }
}
