using System.Threading.Tasks;
using Hub.Interfaces;
using Hub.Services;
using Moq;
using NUnit.Framework;
using PlanDirectory.Interfaces;

namespace PlanDirectoryTests.Infrastructure
{
    [TestFixture]
    public class ManifestPageGeneratorTests
    {
        private Mock<ITemplateGenerator> _templateGeneratorMock;
        private Mock<IPageDefinition> _pageDefinitionServiceMock;

        [Test]
        public async Task Generate_WhenPageDoesntExistAndRegenerationIsNotRequested_ThrowsException()
        {
            
        }
    }
}
