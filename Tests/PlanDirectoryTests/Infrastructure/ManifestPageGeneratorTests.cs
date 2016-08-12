using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Interfaces;
using Moq;
using NUnit.Framework;
using Hub.Exceptions;
using Hub.Enums;
using Hub.Services.PlanDirectory;
using HubWeb.Infrastructure_PD.TemplateGenerators;

namespace PlanDirectoryTests.Infrastructure
{
    [TestFixture]
    public class ManifestPageGeneratorTests
    {
        private Mock<ITemplateGenerator> _templateGeneratorMock;
        private Mock<IPageDefinition> _pageDefinitionServiceMock;
        private Mock<IFr8Account> _fr8AccountServiceMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [SetUp]
        public void SetUp()
        {
            _templateGeneratorMock = new Mock<ITemplateGenerator>();
            _pageDefinitionServiceMock = new Mock<IPageDefinition>();
            _fr8AccountServiceMock = new Mock<IFr8Account>();
            _fr8AccountServiceMock.Setup(x => x.GetSystemUser()).Returns(new Fr8AccountDO(new EmailAddressDO("xx@xx.xx")));
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();
            _unitOfWorkFactoryMock.Setup(x => x.Create()).Returns(_unitOfWorkMock.Object);

        }

        private ManifestPageGenerator CreateTarget()
        {
            return new ManifestPageGenerator(
                _templateGeneratorMock.Object,
                _pageDefinitionServiceMock.Object,
                _fr8AccountServiceMock.Object,
                _unitOfWorkFactoryMock.Object);
        }

        [Test]
        [ExpectedException(typeof(ManifestPageNotFoundException), UserMessage = "Exception is not thrown when page doesn't exist")]
        public async Task Generate_WhenPageDoesntExistAndRegenerationIsNotRequested_ThrowsException()
        {
            _pageDefinitionServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<PageDefinitionDO, bool>>>()))
                .Returns(new List<PageDefinitionDO>(0));
            var target = CreateTarget();
            await target.Generate("Name", GenerateMode.RetrieveExisting);
        }

        [Test]
        public async Task Generate_WhenPageExistsAndRegenerationIsNotForced_ReturnsExistingPage()
        {
            _pageDefinitionServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<PageDefinitionDO, bool>>>()))
                .Returns(new List<PageDefinitionDO> { new PageDefinitionDO { Url = new Uri("http://example.com") } });
            var target = CreateTarget();
            var result = await target.Generate("Name", GenerateMode.GenerateIfNotExists);
            Assert.AreEqual(new Uri("http://example.com"), result, "Existing page was not returned when regeneration was not forced");
        }

        [Test]
        [ExpectedException(typeof(ManifestGenerationException), UserMessage = "Exception is not thrown when system user doesn't exist")]
        public async Task Generate_WhenSystemUserDoesntExist_ThrowsException()
        {
            _fr8AccountServiceMock.Setup(x => x.GetSystemUser()).Returns((Fr8AccountDO)null);
            _pageDefinitionServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<PageDefinitionDO, bool>>>()))
                .Returns(new List<PageDefinitionDO>(0));
            var target = CreateTarget();
            await target.Generate("Name", GenerateMode.GenerateAlways);
        }

        [Test]
        [ExpectedException(typeof(ManifestNotFoundException), UserMessage = "Exception is not thrown when manifest doesn't exist")]
        public async Task Generate_WhenManifestDoesntExist_ThrowsException()
        {
            _pageDefinitionServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<PageDefinitionDO, bool>>>()))
               .Returns(new List<PageDefinitionDO>(0));
            _unitOfWorkMock.Setup(x => x.MultiTenantObjectRepository)
                .Returns(() =>
                {
                    var mock = new Mock<IMultiTenantObjectRepository>();
                    mock.Setup(x => x.Query(It.IsAny<string>(), It.IsAny<Expression<Func<ManifestDescriptionCM, bool>>>()))
                        .Returns(new List<ManifestDescriptionCM>(0));
                    return mock.Object;
                });
            var target = CreateTarget();
            await target.Generate("Name", GenerateMode.GenerateAlways);
        }

        [Test]
        public async Task Generate_WhenManifestExists_SavesPageDefinitionAndGeneratesFile()
        {
            _pageDefinitionServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<PageDefinitionDO, bool>>>()))
                .Returns(new List<PageDefinitionDO>(0));
            _unitOfWorkMock.Setup(x => x.MultiTenantObjectRepository)
               .Returns(() =>
               {
                   var mock = new Mock<IMultiTenantObjectRepository>();
                   mock.Setup(x => x.Query(It.IsAny<string>(), It.IsAny<Expression<Func<ManifestDescriptionCM, bool>>>()))
                       .Returns(new List<ManifestDescriptionCM> { new ManifestDescriptionCM { Name = "Name"} });
                   return mock.Object;
               });
            _templateGeneratorMock.Setup(x => x.BaseUrl).Returns(new Uri("http://example.com"));
            _templateGeneratorMock.Setup(x => x.OutputFolder).Returns("C:\\");
            var target = CreateTarget();
            await target.Generate("Name", GenerateMode.GenerateAlways);
            _pageDefinitionServiceMock.Verify(x => x.CreateOrUpdate(It.IsAny<PageDefinitionDO>()), Times.Once(), "Page definition was not saved");
            _templateGeneratorMock.Verify(x => x.Generate(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), 
                Times.Once(),
                "File was not generated");
        }
    }
}
