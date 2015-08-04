using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Principal;
using System.Web.Http.Results;
using Core.Interfaces;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using StructureMap.AutoMocking;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("Controllers.Api.ProcessTemplateService")]
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
            ProcessTemplateDTO processTemplateDto;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                processTemplateDto = new FixtureData(uow).TestProcessTemplateDTO();
            }

            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            var response = ptc.Post(processTemplateDto);

            //Assert
            var okResult = response as OkNegotiatedContentResult<ProcessTemplateDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.ProcessTemplateRepository.
                    GetQuery().SingleOrDefault(pt => pt.UserId == testUserId && pt.Name == processTemplateDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(processTemplateDto.Description, ptdo.Description);
            }
        }

        [Test]
        public void ProcessTemplateController_Will_Return_BadResult_If_Name_Is_Empty()
        {
            //Arrange 
            string testUserId = "testuser";
            ProcessTemplateDTO processTemplateDto;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                processTemplateDto = new FixtureData(uow).TestEmptyNameProcessTemplateDTO();
            }


            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(testUserId);
            var response = ptc.Post(processTemplateDto);

            //Assert
            var badResult = response as BadRequestErrorMessageResult;
            Assert.NotNull(badResult);

        }

        [Test]
        public void ProcessTemplateController_Will_ThrowException_If_No_ProcessTemplate_Found()
        {
            //Arrange 
            string testUserId = "testuser";



            //Act
            ProcessTemplateController processTemplateController = CreateProcessTemplateController(testUserId);


            //Assert

            Assert.Throws<ApplicationException>(() =>
            {
                processTemplateController.Get(55);

            }, "Process Template not found for id 55");

        }

        [Test]
        public void ProcessController_Will_Return_All_When_Get_Invoked_With_Null()
        {
            //Arrange
            var testUserId = "testuser1";

            var processTemplateController = CreateProcessTemplateController(testUserId);


            for (var i = 0; i < 4; i++)
            {
                ProcessTemplateDTO processTemplateDto;
                using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    processTemplateDto = new FixtureData(unitOfWork).TestProcessTemplateDTO();
                }
                processTemplateController.Post(processTemplateDto);

            }

            //Act
            var actionResult = processTemplateController.Get() as OkNegotiatedContentResult<IEnumerable<ProcessTemplateDTO>>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.AreEqual(4, actionResult.Content.Count());

        }

        [Test]
        public void ProcessController_Will_Return_One_When_Get_Invoked_With_Id()
        {
            //Arrange
            var testUserId = "testuser4";

            var processTemplateController = CreateProcessTemplateController(testUserId);



            ProcessTemplateDTO processTemplateDto;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                processTemplateDto = new FixtureData(uow).TestProcessTemplateDTO();
            }
            processTemplateController.Post(processTemplateDto);



            //Act
            var actionResult = processTemplateController.Get(processTemplateDto.Id) as OkNegotiatedContentResult<ProcessTemplateDTO>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Content);
            Assert.AreEqual(processTemplateDto.Id, actionResult.Content.Id);

        }

        [Test]
        public void ProcessTemplateController_CanDelete()
        {
            //Arrange 
            string testUserId = "testuser3";
            int id = 3;
            ProcessTemplateDTO processTemplateDto;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                processTemplateDto = new FixtureData(uow).TestProcessTemplateDTO();
            }

            ProcessTemplateController processTemplateController = CreateProcessTemplateController(testUserId);
            var postResult = processTemplateController.Post(processTemplateDto) as OkNegotiatedContentResult<ProcessTemplateDTO>;


            Assert.NotNull(postResult);

            //Act
            var deleteResult = processTemplateController.Delete(postResult.Content.Id) as OkResult;

            Assert.NotNull(deleteResult);

            //Assert
            Assert.Throws<ApplicationException>(() =>
            {
                processTemplateController.Get(postResult.Content.Id);

            }, "Process Template not found for id " + postResult.Content.Id);
        }


        //[Test]
        //public void ProcessController_CannotCreateIfProcessNameIsEmpty()
        //{
        //    //Arrange 
        //    string testUserId = "testuser";
        //    var ptvm = new ProcessTemplateDTO();
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
        //    var ptvm = new ProcessTemplateDTO();
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
        //    //ptvm = (result as OkNegotiatedContentResult<ProcessTemplateDTO>).Content;
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

            var ptc = new ProcessTemplateController
            {
                User = new GenericPrincipal(new GenericIdentity(testUserId, "Forms"), new[] { "USers" })
            };
            return ptc;
        }
    }
}
