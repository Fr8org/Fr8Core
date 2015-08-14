using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Core;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest
{
    [TestFixture]
    public class TemplateServiceTests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void Can_Return_List_Of_Fields()
        {
            var fields = (new Core.Services.Template())
                                .GetMappableSourceFields(FixtureData.TestTeamplateId).ToList();

            Assert.IsNotNull(fields);
            Console.Write(JsonConvert.SerializeObject(fields));
        }

    }
}