using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Core;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Entities;
using Core.Services;
using Data.Wrappers;

namespace DockyardTest
{
    [TestFixture]
    public class TemplateTests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        [Ignore]
        // This is more of an integration test
        // test this to make sure docusign is returning values
        // Template id is "b5abd63a-c12c-4856-b9f4-989200e41a6f"
        // <add key="username" value="a@thakral.in" />
        // <add key = "password" value="foobar1" />
        // <add key = "IntegratorKey" value="TEST-ddb13d45-cc4f-4573-9c37-f75712565ed1" />
        // <add key = "BaseUrl" value="https://demo.docusign.net/restapi/v2/accounts/1142188/" />

        public void Can_Return_List_Of_Fields()
        {
            var fields = (new DocuSignTemplate())
                                .GetMappableSourceFields(FixtureData.TestTemplateId).ToList();

            Assert.IsNotNull(fields);
            Console.Write(JsonConvert.SerializeObject(fields));
        }

    }
}