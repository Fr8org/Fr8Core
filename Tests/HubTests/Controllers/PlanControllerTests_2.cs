using System;
using System.Web.Http.Results;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.Controllers;
using HubWeb.ViewModels;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities.Interfaces;
using System.Threading.Tasks;
using Data.Constants;
using Data.States;
using HubTests.Services;
using HubTests.Services.Container;
using UtilitiesTesting.Fixtures;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("PlanControllerTests")]
    public class PlanControllerTests_2 : ContainerExecutionTestBase
    {
        [Test]
        public async Task CanContinueExecutionAfterSuspend()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO plan;

                uow.PlanRepository.Add(plan = new PlanDO
                {
                    Name = "TestPlan",
                    Id = FixtureData.GetTestGuidById(0),
                    ChildNodes =
                    {
                        new SubPlanDO(true)
                        {
                            Id = FixtureData.GetTestGuidById(1),
                            ChildNodes =
                            {
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(2),
                                    Ordering = 1
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(3),
                                    Ordering = 2
                                },
                                new ActivityDO()
                                {
                                    ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                    Id = FixtureData.GetTestGuidById(4),
                                    Ordering = 3
                                },
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new SuspenderActivityMock(CrateManager);

                plan.PlanState = PlanState.Active;
                plan.StartingSubPlan = (SubPlanDO)plan.ChildNodes[0];

                uow.SaveChanges();

                var controller = new PlansController();
                // Act
                var container = await controller.Run(plan.Id, null);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);

                Assert.NotNull(container); // Get not empty result
                Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(container); // Result of correct HTTP response type with correct payload

                container = await controller.Run(plan.Id, null, ((OkNegotiatedContentResult<ContainerDTO>)container).Content.Id);

                Assert.NotNull(container); // Get not empty result
                Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(container); // Result of correct HTTP response type with correct payload

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(4)),
                }, ActivityService.ExecutedActivities);
            }
        }


        [Test]
        public void PlanController_RunCanBeExecutedWithoutPayload()
        {
            // Arrange
            Mock<IPlanRepository> rrMock = new Mock<IPlanRepository>();
            rrMock.Setup(x => x.GetById<PlanDO>(It.IsAny<Guid>())).Returns(new PlanDO()
            {
                Fr8Account = FixtureData.TestDockyardAccount1(),
                StartingSubPlan = new SubPlanDO()
            });

            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

            Mock<IPlan> planMock = new Mock<IPlan>();
            planMock.Setup(x => x.Run(It.IsAny<PlanDO>(), It.IsAny<Crate>())).ReturnsAsync(new ContainerDO());

            Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
            pusherMock.Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()));


            ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
            ObjectFactory.Container.Inject(typeof(IPlan), planMock.Object);
            ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

            var controller = new PlansController();

            // Act
            var result = controller.Run(Guid.NewGuid(), null);

            // Assert
            Assert.NotNull(result.Result);                                                  // Get not empty result
            Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(result.Result);    // Result of correct HTTP response type with correct payload
        }

        [Test]
        public void PlanController_RunWouldReturn400WhenCalledWithInvalidPayload()
        {
            // Arrange
            Mock<IPlanRepository> rrMock = new Mock<IPlanRepository>();
            rrMock.Setup(x => x.GetById<PlanDO>(It.IsAny<Guid>())).Returns(new PlanDO()
            {
                StartingSubPlan = new SubPlanDO()
            });

            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

            Mock<IPlan> planMock = new Mock<IPlan>();
            planMock.Setup(x => x.Run(It.IsAny<PlanDO>(), It.IsAny<Crate>())).ReturnsAsync(new ContainerDO());

            Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
            pusherMock.Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()));

            ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
            ObjectFactory.Container.Inject(typeof(IPlan), planMock.Object);
            ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

            var controller = new PlansController();

            // Act
            var result = controller.Run(Guid.NewGuid(), new PayloadVM { Payload = "Some crap data" });

            // Assert
            Assert.NotNull(result.Result);                              // Get not empty result
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result.Result);       // Result of correct HTTP response type
        }

        [Test]
        public void PlanController_RunWouldBeExecutedWithAValidPayload()
        {
            // Arrange
            Mock<IPlanRepository> rrMock = new Mock<IPlanRepository>();
            rrMock.Setup(x => x.GetById<PlanDO>(It.IsAny<Guid>())).Returns(new PlanDO()
            {
                Fr8Account = FixtureData.TestDockyardAccount1(),
                StartingSubPlan = new SubPlanDO()
            });

            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

            Mock<IPlan> planMock = new Mock<IPlan>();
            planMock.Setup(x => x.Run(It.IsAny<PlanDO>(), It.IsAny<Crate>())).ReturnsAsync(new ContainerDO());

            Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
            pusherMock.Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()));


            ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
            ObjectFactory.Container.Inject(typeof(IPlan), planMock.Object);
            ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

            var controller = new PlansController();

            // Act
            var payload = new PayloadVM
            {
                Payload =
                    "{ \"id\": \"eb2b56e9-daa2-4a7c-bfa2-20cea72b2302\", " +
                    "\"label\": \"Upstream Terminal-Provided Fields\", " +
                    "\"contents\": { " +
                    "\"Fields\": [ " +
                    "{ \"key\": \"Medical_Form_v2\", \"value\": \"ea2258b2-2d80-4eca-9f40-6c5b5d5c5dda\"}," +
                    "{ \"key\": \"Template_For_DocuSignTemplateTests\", \"value\": \"9a318240-3bee-475c-9721-370d1c22cec4\" }, " +
                    "{ \"key\": \"Untitled Oct 29th 2015\", \"value\": \"a4fba1d5-9fad-41ab-9e23-f5ad5f4097df\" }, " +
                    "{ \"key\": \"Medical_Form_v1\", \"value\": \"58521204-58af-4e65-8a77-4f4b51fef626\" }, " +
                    "{ \"key\": \"Untitled Oct 16th 2015\", \"value\": \"5dac4b56-89af-435b-b7b9-a8d0c8922e37\" }," +
                    "{ \"key\": \"\", \"value\": \"6b1aaa7d-94a3-40a1-a091-1360a2032e23\" }, " +
                    "{ \"key\": \"EnvelopeId\", \"value\": \"\" }] " +
                    "}, " +
                    "\"parentCrateId\": null, " +
                    "\"manifestType\": \"Standard Design-Time Fields\"," +
                    "\"manifestId\": 3," +
                    "\"manufacturer\": null, " +
                    "\"createTime\": \"0001-01-01T00:00:00\" }"
            };

            var result = controller.Run(Guid.NewGuid(), payload);
            // Assert
            Assert.NotNull(result.Result);                                                  // Get not empty result
            Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(result.Result);    // Result of correct HTTP response type with correct payload
        }
    }
}