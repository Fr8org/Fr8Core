using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Moq;
using NUnit.Framework;
using StructureMap;
using Web.Controllers;
using Web.ViewModels;
using System.Linq;
using Data.States;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Ignore]
    [Category("Controllers.ProcessTemplate")]
    public class ProcessTemplateControllerTests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void ProcessController_CanAddNewProcessTemplate()
        {
            //Arrange 
            string testUserId = "testuser";
            var ptvm = new ProcessTemplateVM();
            ptvm.Name = "processtemplate1";
            ptvm.Description = "Description for test process template";

            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            ptc.Create(ptvm);

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(ptvm.Description, ptdo.Description);
            }
        }

        [Test]
        public void ProcessController_CannotCreateIfProcessNameIsEmpty()
        {
            //Arrange 
            string testUserId = "testuser";
            var ptvm = new ProcessTemplateVM();
            ptvm.Description = "Description for test process template";

            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            ptc.Create(ptvm);

            //Assert
            Assert.AreEqual(1, ptc.ModelState.Count()); //must be one error
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(ptvm.Description, ptdo.Description);
            }
        }

        [Test]
        public void ProcessController_CanEditProcess()
        {
            //Arrange 
            ViewResult result;
            string testUserId = "testuser2";
            int id = 2;
            var ptvm = new ProcessTemplateVM();
            ptvm.Description = "Description for test process template";
            ptvm.Name = "processtemplate1";
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            ptc.Create(ptvm);

            //Manually specify Id since mocked repository does not generate id value automatically
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                ptdo.Id = id;
                uow.SaveChanges();
            }

            //Simulate editing Process Temlate
            //Manually specify Id since mocked repository does not generate id value automatically
            ptc = CreateProcessTemplateController(testUserId);

            //Act
            result = ptc.Edit(id) as ViewResult; // get view model for id 
            ptvm = result.ViewData.Model as ProcessTemplateVM;
            ptvm.Name = "processtemplate_edited";
            ptvm.Description = "Description for test process template edited";

            result = ptc.Edit(ptvm) as ViewResult; //edit record

            //Assert
            Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetByKey(id);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(ptvm.Name, ptdo.Name);
                Assert.AreEqual(ptvm.Description, ptdo.Description);
            }
        }

        [Test]
        public void ProcessController_CanDeleteProcess()
        {
            //Arrange 
            string testUserId = "testuser3";
            int id = 3;
            var ptvm = new ProcessTemplateVM();
            ptvm.Description = "Description for test process template";
            ptvm.Name = "processtemplate1";
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            ptc.Create(ptvm);

            //Manually specify Id since mocked repository does not generate id value automatically
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                ptdo.Id = id;
                uow.SaveChanges();
            }

            //pre-Assert: must be one record
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                Assert.IsNotNull(ptdo);
            }

            //Act
            ptc.Delete(id);

            //Assert: must be no records
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                Assert.IsNull(ptdo);
            }
        }

        [Test]
        public void ProcessController_CanShowAllProcesses()
        {
            ProcessTemplateDO ptdo;
            string testUserId = "testuser4";

            //Arrange 
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                for (int i = 0; i < 10; i++)
                {
                    ptdo = new ProcessTemplateDO();
                    ptdo.UserId = testUserId;
                    ptdo.Name = "Process template " + i.ToString();
                    ptdo.ProcessState = ProcessTemplateState.Active;
                    ptdo.Description = "Process template descrption " + i.ToString();
                    uow.ProcessTemplateRepository.Add(ptdo);
                }
                uow.SaveChanges();
            }

            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            var result = ptc.Index() as ViewResult; // get view model for id 
            var ptvm = result.ViewData.Model as IEnumerable<ProcessTemplateVM>;

            //Assert
            Assert.AreEqual(10, ptvm.ToList().Count);
        }

        private static ProcessTemplateController CreateProcessTemplateController(string testUserId)
        {
            var ctxMock = new Mock<ControllerContext>();
            ctxMock.SetupGet(p => p.HttpContext.User.Identity.Name).Returns(testUserId);
            ctxMock.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);

            var ptc = new ProcessTemplateController();
            ptc.ControllerContext = ctxMock.Object;
            return ptc;
        }
    }
}
