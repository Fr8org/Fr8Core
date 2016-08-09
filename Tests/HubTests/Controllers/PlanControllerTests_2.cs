using System;
using System.Web.Http.Results;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using HubWeb.Controllers;
using Moq;
using NUnit.Framework;
using StructureMap;
using System.Threading.Tasks;
using Data.States;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using HubTests.Services.Container;
using Fr8.Testing.Unit.Fixtures;
using Fr8.Infrastructure.Data.States;

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
                        new SubplanDO(true)
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
                                }
                            }
                        }
                    }
                });

                ActivityService.CustomActivities[FixtureData.GetTestGuidById(3)] = new SuspenderActivityMock(CrateManager);

                plan.PlanState = PlanState.Executing;
                plan.StartingSubplan = (SubplanDO)plan.ChildNodes[0];
                var userAcct = FixtureData.TestUser1();
                uow.UserRepository.Add(userAcct);
                plan.Fr8Account = userAcct;
                uow.SaveChanges();

                var controller = ObjectFactory.GetInstance<PlansController>();
                // Act
                var container = await controller.Run(plan.Id, null);

                AssertExecutionSequence(new[]
                {
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(2)),
                    new ActivityExecutionCall(ActivityExecutionMode.InitialRun, FixtureData.GetTestGuidById(3)),
                }, ActivityService.ExecutedActivities);

                Assert.NotNull(container); // Get not empty result
                Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(container); // Result of correct HTTP response type with correct payload
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
                StartingSubplan = new SubplanDO()
            });

            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

            Mock<IPlan> planMock = new Mock<IPlan>();
            planMock.Setup(x => x.Run(It.IsAny<Guid>(), It.IsAny<Crate[]>(), It.IsAny<Guid?>())).ReturnsAsync(new ContainerDTO());
            planMock.Setup(x => x.Activate(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new ActivateActivitiesDTO());

            planMock.Setup(x=> x.GetFullPlan(uowMock.Object, (It.IsAny<Guid>()))).Returns(new PlanDO()
            {
                Fr8Account = FixtureData.TestDockyardAccount1(),
                StartingSubplan = new SubplanDO()
            });

            Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
            pusherMock.Setup(x => x.NotifyUser(It.IsAny<NotificationMessageDTO>(), It.IsAny<string>()));

            ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
            ObjectFactory.Container.Inject(typeof(IPlan), planMock.Object);
            ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

            var controller = ObjectFactory.GetInstance<PlansController>();

            // Act
            var result = controller.Run(Guid.NewGuid(), null);

            // Assert
            Assert.NotNull(result.Result);                                                  // Get not empty result
            Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(result.Result);    // Result of correct HTTP response type with correct payload
        }

       
        [Test]
        public void PlanController_RunWouldBeExecutedWithAValidPayload()
        {
            // Arrange
            Mock<IPlanRepository> rrMock = new Mock<IPlanRepository>();
            rrMock.Setup(x => x.GetById<PlanDO>(It.IsAny<Guid>())).Returns(new PlanDO()
            {
                Fr8Account = FixtureData.TestDockyardAccount1(),
                StartingSubplan = new SubplanDO()
            });

            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

            Mock<IPlan> planMock = new Mock<IPlan>();
            planMock.Setup(x => x.Run(It.IsAny<Guid>(), It.IsAny<Crate[]>(), It.IsAny<Guid?>())).ReturnsAsync(new ContainerDTO());
            planMock.Setup(x => x.Activate(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new ActivateActivitiesDTO());
            planMock.Setup(x => x.GetFullPlan(uowMock.Object, (It.IsAny<Guid>()))).Returns(new PlanDO()
            {
                Fr8Account = FixtureData.TestDockyardAccount1(),
                StartingSubplan = new SubplanDO()
            });

            Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
            pusherMock.Setup(x => x.NotifyUser(It.IsAny<NotificationMessageDTO>(), It.IsAny<string>()));

            ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
            ObjectFactory.Container.Inject(typeof(IPlan), planMock.Object);
            ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

            var controller = ObjectFactory.GetInstance<PlansController>();

            var crate = Crate.FromContent("Payload", new StandardPayloadDataCM(new KeyValueDTO("I'm", "payload")));

            var result = controller.Run(Guid.NewGuid(), new[] {CrateStorageSerializer.Default.ConvertToDto(crate)});
            // Assert
            Assert.NotNull(result.Result);                                                  // Get not empty result
            Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(result.Result);    // Result of correct HTTP response type with correct payload
        }
    }
}