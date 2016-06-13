using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PlanDirectory.Infrastructure;

namespace terminalIntegrationTests.Unit
{
    public class PageGeneratorTests
    {
        [Test]
        public void SutIsPageGenerator()
        {
            var sut = new PlanCategoryPageGenerator();
            Assert.IsInstanceOf<IPageGenerator>(sut);
        }

        static IEnumerable<IEnumerable<string>> Tags = new List<List<string>>
        {
            new List<string>{"slack", "ms-excel"},
            new List<string>{"google", "dropbox", "twilio"}
        };

        [Explicit("Just for fast running generator. Not test anything")]
        [Test, TestCaseSource(nameof(Tags))]
        public void ShouldGeneratePagesFromTags(IEnumerable<string> tags)
        {
            var sut = new PlanCategoryPageGenerator();
            sut.Generate(tags);
        }
    }
}
