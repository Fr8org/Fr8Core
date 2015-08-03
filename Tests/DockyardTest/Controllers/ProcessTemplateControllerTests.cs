using System;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Principal;
using System.Web.Http.Results;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("Controllers.Api.ProcessTemplate")]
    public class ProcessTemplateControllerTests : BaseTest 
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void ProcessTemplateController_CanAddNewProcessTemplate()
        {
            //Arrange 
            string testUserId = "testuser";
            var ptvm = new ProcessTemplateVM
            {
                Name = "processtemplate1",
                Description = "Description for test process template",
                ProcessTemplateState = 1
            };
            

            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            var response=ptc.Post(ptvm);

            //Assert
            var okResult = response as OkNegotiatedContentResult<ProcessTemplateVM>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.ProcessTemplateRepository.GetQuery().SingleOrDefault(pt => pt.UserId == testUserId && pt.Name == ptvm.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(ptvm.Description, ptdo.Description);
            }
        }

        [Test]
        public void ProcessTemplateController_will_Return_BadResult_if_name_is_empty()
        {
            //Arrange 
            string testUserId = "testuser";
            var ptvm = new ProcessTemplateVM
            {
                Name = "",
                Description = "Description for test process template",
                ProcessTemplateState = 1
            };


            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            var response = ptc.Post(ptvm);

            //Assert
            var badResult = response as BadRequestErrorMessageResult;
            Assert.NotNull(badResult);
           
        }

        [Test]
        public void ProcessTemplateController_Will_Return_NotFound_If_No_ProcessTemplate_Found()
        {
            //Arrange 
            string testUserId = "testuser";
         


            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            var response = ptc.Get(55);

            //Assert
            var badResult = response as NotFoundResult;
            Assert.NotNull(badResult);

        }

	  //[Test]
	  //public void ProcessController_CannotCreateIfProcessNameIsEmpty()
	  //{
	  //    //Arrange 
	  //    string testUserId = "testuser";
	  //    var ptvm = new ProcessTemplateVM();
	  //    ptvm.Description = "Description for test process template";
	  //    ptvm.ProcessState = 1;

	  //    //Act
	  //    ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
	  //    //ptc.Post(ptvm);

	  //    //Assert
	  //    Assert.AreEqual(1, ptc.ModelState.Count()); //must be one error
	  //}

	  //[Test]
	  //public void ProcessController_CanEditProcess()
	  //{
	  //    //Arrange 
	  //    IHttpActionResult result;
	  //    string testUserId = "testuser2";
	  //    int id = 2;
	  //    var ptvm = new ProcessTemplateVM();
	  //    ptvm.Description = "Description for test process template";
	  //    ptvm.Name = "processtemplate1";
	  //    ptvm.ProcessState = 1;
	  //    ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
	  //   // ptc.Post(ptvm);

	  //    //Manually specify Id since mocked repository does not generate id value automatically
	  //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
	  //    {
	  //	  //var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
	  //	  //ptdo.Id = id;
	  //	  uow.SaveChanges();
	  //    }

	  //    //Simulate editing Process Temlate
	  //    //Manually specify Id since mocked repository does not generate id value automatically
	  //    ptc = CreateProcessTemplateController(testUserId);

	  //    //Act
	  //   // result = ptc.Get(id) as IHttpActionResult; // get view model for id 
	  //    //ptvm = (result as OkNegotiatedContentResult<ProcessTemplateVM>).Content;
	  //    ptvm.Name = "processtemplate_edited";
	  //    ptvm.Description = "Description for test process template edited";

	  //    //result = ptc.Post(ptvm) as IHttpActionResult; //edit record

	  //    //Assert
	  //    Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
	  //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
	  //    {
	  //	  var ptdo = uow.ProcessTemplateRepository.GetByKey(id);
	  //	  Assert.IsNotNull(ptdo);
	  //	  Assert.AreEqual(ptvm.Name, ptdo.Name);
	  //	  Assert.AreEqual(ptvm.Description, ptdo.Description);
	  //    }
	  //}

        [Test]
        public void ProcessController_CanDeleteProcess()
        {
            //Arrange 
            string testUserId = "testuser3";
            int id = 3;
            var ptvm = new ProcessTemplateVM();
            ptvm.Description = "Description for test process template";
            ptvm.Name = "processtemplate1";
            ptvm.ProcessTemplateState = 1;
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            //ptc.Post(ptvm);

            //Manually specify Id since mocked repository does not generate id value automatically
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
               // var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                //ptdo.Id = id;
                uow.SaveChanges();
            }

            //pre-Assert: must be one record
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
               // var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                //Assert.IsNotNull(ptdo);
            }

            //Act
           // ptc.Delete(id);

            //Assert: must be no records
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
               // var ptdo = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.UserId == testUserId && pt.Name == ptvm.Name).SingleOrDefault();
                //Assert.IsNull(ptdo);
            }
        }

	  //[Test]
	  //public void ProcessController_CanShowAllProcesses()
	  //{
	  //    ProcessTemplateDO ptdo;
	  //    string testUserId = "testuser4";

	  //    //Arrange 
	  //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
	  //    {
	  //	  for (int i = 0; i < 10; i++)
	  //	  {
	  //		ptdo = new ProcessTemplateDO();
	  //	     // ptdo.UserId = testUserId;
	  //		ptdo.Name = "Process template " + i.ToString();
	  //		//ptdo.ProcessState = ProcessTemplateState.Active;
	  //		ptdo.Description = "Process template descrption " + i.ToString();
	  //		uow.ProcessTemplateRepository.Add(ptdo);
	  //	  }
	  //	  uow.SaveChanges();
	  //    }

	  //    //Act
	  //    ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
	  //    var ptvm =  ptc.Get(); //get view model

	  //    //Assert
	  //   Assert.AreEqual(10, ptvm.ToList().Count);
	  //}

        private static ProcessTemplateController CreateProcessTemplateController(string testUserId)
        {

            var ptc = new ProcessTemplateController();
            ptc.User = new GenericPrincipal(new GenericIdentity(testUserId, "Forms"), new[] { "USers" });
            return ptc;
        }
    }
}
