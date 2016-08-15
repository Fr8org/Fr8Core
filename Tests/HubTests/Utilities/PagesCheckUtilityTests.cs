using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Testing.Unit;
using Hub.Interfaces;
using Hub.Services.PlanDirectory;
using HubWeb.Infrastructure_PD;
using Moq;
using NUnit.Framework;

namespace HubTests.Utilities
{
    [TestFixture]
    class PagesCheckUtilityTests: BaseTest
    {
        private PagesCheckUtility _pagesCheckUtility;
        private IPlanTemplateDetailsGenerator _planTemplateDetailsGenerator;
        private ISearchProvider _searchProvider;


        [TestFixtureSetUp]
        public void Start()
        {
            _searchProvider = new SearchProvider();
            var _planTemplateDetailsGeneratorMock = new Mock<IPlanTemplateDetailsGenerator>();
            _planTemplateDetailsGenerator = _planTemplateDetailsGeneratorMock.Object;

            _pagesCheckUtility = new PagesCheckUtility(_planTemplateDetailsGenerator, _searchProvider);
        }

        [Test]
        public async Task Should_creat_plan_file_if_not_exists()
        {


            return;
        }

    }
}
