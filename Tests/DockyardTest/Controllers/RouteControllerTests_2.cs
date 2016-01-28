using System;
using System.Web.Http.Results;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories;
using Hub.Interfaces;
using HubWeb.Controllers;
using HubWeb.ViewModels;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities.Interfaces;
using UtilitiesTesting;

namespace DockyardTest.Controllers
{
	[TestFixture]
	[Category("RouteControllerTests")]
	public class RouteControllerTests_2 : BaseTest
	{
		[Test]
		public void RouteController_RunCanBeExecutedWithoutPayload()
		{
			// Arrange
			Mock<IPlanRepository> rrMock = new Mock<IPlanRepository>();
			rrMock.Setup(x => x.GetByKey(It.IsAny<Guid>())).Returns(new PlanDO());

			Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
			uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

			Mock<IPlan> routeMock = new Mock<IPlan>();
			routeMock.Setup(x => x.Run(It.IsAny<PlanDO>(), It.IsAny<Crate>())).ReturnsAsync(new ContainerDO());

			Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
			pusherMock.Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()));

			ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
			ObjectFactory.Container.Inject(typeof(IPlan), routeMock.Object);
			ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

			var controller = new RoutesController();

			// Act
			var result = controller.Run(Guid.NewGuid(), null);

			// Assert
			Assert.NotNull(result.Result);													// Get not empty result
			Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(result.Result);	// Result of correct HTTP response type with correct payload
		}

		[Test]
		public void RouteController_RunWouldReturn400WhenCalledWithInvalidPayload()
		{
			// Arrange
			Mock<IPlanRepository> rrMock = new Mock<IPlanRepository>();
			rrMock.Setup(x => x.GetByKey(It.IsAny<Guid>())).Returns(new PlanDO());

			Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
			uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

			Mock<IPlan> routeMock = new Mock<IPlan>();
			routeMock.Setup(x => x.Run(It.IsAny<PlanDO>(), It.IsAny<Crate>())).ReturnsAsync(new ContainerDO());

			Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
			pusherMock.Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()));

			ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
			ObjectFactory.Container.Inject(typeof(IPlan), routeMock.Object);
			ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

			var controller = new RoutesController();

			// Act
			var result = controller.Run(Guid.NewGuid(), new PayloadVM { Payload = "Some crap data" });

			// Assert
			Assert.NotNull(result.Result);								// Get not empty result
			Assert.IsInstanceOf<BadRequestResult>(result.Result);		// Result of correct HTTP response type
		}

		[Test]
		public void RouteController_RunWouldBeExecutedWithAValidPayload()
		{
			// Arrange
			Mock<IPlanRepository> rrMock = new Mock<IPlanRepository>();
			rrMock.Setup(x => x.GetByKey(It.IsAny<Guid>())).Returns(new PlanDO());

			Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
			uowMock.Setup(x => x.PlanRepository).Returns(rrMock.Object);

			Mock<IPlan> routeMock = new Mock<IPlan>();
			routeMock.Setup(x => x.Run(It.IsAny<PlanDO>(), It.IsAny<Crate>())).ReturnsAsync(new ContainerDO());

			Mock<IPusherNotifier> pusherMock = new Mock<IPusherNotifier>();
			pusherMock.Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()));

			ObjectFactory.Container.Inject(typeof(IUnitOfWork), uowMock.Object);
			ObjectFactory.Container.Inject(typeof(IPlan), routeMock.Object);
			ObjectFactory.Container.Inject(typeof(IPusherNotifier), pusherMock.Object);

			var controller = new RoutesController();

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
			Assert.NotNull(result.Result);													// Get not empty result
			Assert.IsInstanceOf<OkNegotiatedContentResult<ContainerDTO>>(result.Result);	// Result of correct HTTP response type with correct payload
		}
	}
}