using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;

using DocuSign.Integrations.Client;

using NUnit.Framework;
using Utilities;

using UtilitiesTesting;
using UtilitiesTesting.DocusignTools;
using UtilitiesTesting.DocusignTools.Interfaces;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;
using Data.Migrations;
using Data.Wrappers;
using Newtonsoft.Json;
using StructureMap;

namespace DockyardTest.DocuSign
{
    [TestFixture]
    public class DocuSignTemplateTests : BaseTest
    {
		 private readonly string TEMPLATE_WITH_ROLES_ID = "9a318240-3bee-475c-9721-370d1c22cec4";
		 private readonly string TEMPLATE_WITH_USER_FIELDS_ID = "9a318240-3bee-475c-9721-370d1c22cec4";
		 private readonly IDocusignApiHelper docusignApiHelper;
        private  IDocuSignTemplate _docusignTemplate;

        public DocuSignTemplateTests()
        {
            //docusignApiHelper = new DocusignApiHelper();
            //_docusignTemplate = ObjectFactory.GetInstance<IDocuSignTemplate>();
        }


        [SetUp]
        public void TestSetup()
        {
            _docusignTemplate = ObjectFactory.GetInstance<IDocuSignTemplate>();
        }


        //[Test, Ignore]
        //[Category("DocuSignIntegration")]
        //public void Template_Can_Get_Template()
        //{
        //    DocuSignAccount account = docusignApiHelper.LoginDocusign(FixtureData.TestDocuSignAccount1(),
        //                                                      FixtureData.TestRestSettings1());

        //    TemplateInfo curTemplateInfo = FixtureData.TestDocuSignTemplateInfo1();         
        //    //post the template to the test account
        //    TemplateInfo createdTemplateInfo = _docusignTemplate.Create(curTemplateInfo);

        //    //get it
        //    //TemplateInfo retrievedTemplateInfo = _docusignTemplate.GetTemplate(createdTemplateInfo.Id);

        //    //verify
        //    //delete it

        //    Assert.IsNotNull(account); //Todo orkan: remove back when you completed the EnvelopeService.
        //}


        [Test]
        [Ignore]
        // This is more of an integration test
        // test this to make sure docusign is returning values
        // Template id is "b5abd63a-c12c-4856-b9f4-989200e41a6f"
        // <add key = "DocuSignLoginEmail" value="a@thakral.in" />
        // <add key = "DocuSignLoginPassword" value="foobar1" />
        // <add key = "DocuSignIntegratorKey" value="TEST-ddb13d45-cc4f-4573-9c37-f75712565ed1" />
        // <add key = "environment" value="https://demo.docusign.net/" />
        // <add key = "endpoint" value="https://demo.docusign.net/restapi/v2/" />
        // <add key = "BaseUrl" value="https://demo.docusign.net/restapi/v2/accounts/1142188/" />
        public void Can_Return_List_Of_Fields()
        {
            var fields = _docusignTemplate.GetMappableSourceFields(FixtureData.TestTeamplateId).ToList();

            Assert.IsNotNull(fields);
            Console.Write(JsonConvert.SerializeObject(fields));
        }
		  [Test]
		  public void GetRecipients_ExistsTemplate_ShouldBeOk()
		  {
			  var recipients = _docusignTemplate.GetRecipients(TEMPLATE_WITH_ROLES_ID);

			  Assert.AreEqual(1, recipients.signers.Length);
			  Assert.AreEqual(1, recipients.carbonCopies.Length);
			  var singer = recipients.signers.First();
			  var carbonCopy = recipients.carbonCopies.First();
			  Assert.AreEqual("President", singer.roleName);
			  Assert.AreEqual("Vise President", carbonCopy.roleName);
		  }
		  [Test]
		  public void GetRecipients_NonExistsTemplate_ExpectedException()
		  {
			  var ex = Assert.Throws<InvalidOperationException>(() => _docusignTemplate.GetRecipients(Guid.NewGuid().ToString()));
		  }

		 [Test]
		  public void GetUserFields_ExistsTempate_ShouldBeOk()
		  {
			  var userFields = _docusignTemplate.GetUserFields(TEMPLATE_WITH_USER_FIELDS_ID);
			  var t1 = userFields.Where(x => x.tabLabel == "CustomField1").FirstOrDefault();
			  var t2 = userFields.Where(x => x.tabLabel == "CustomField2").FirstOrDefault();
			  Assert.AreEqual(2, userFields.Count);
			  Assert.NotNull(t1);
			  Assert.NotNull(t2);
		  }
    }
}