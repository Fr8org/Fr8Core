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
	public class DocuSignTextTabTests : BaseTest
	{
		private readonly string TEMPLATE_WITH_USER_FIELDS_ID = "9a318240-3bee-475c-9721-370d1c22cec4";

		public DocuSignTextTabTests()
		{
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
		}

		[Test]
		public void GetUserFields_ExistsTempate_ShouldBeOk()
		{
			DocuSignTextTab docuSignTextTab = new DocuSignTextTab();
			var userFields = docuSignTextTab.GetUserFields(TEMPLATE_WITH_USER_FIELDS_ID);
			var t1 = userFields.Where(x => x.tabLabel == "CustomField1").FirstOrDefault();
			var t2 = userFields.Where(x => x.tabLabel == "CustomField2").FirstOrDefault();
			Assert.AreEqual(2, userFields.Count);
			Assert.NotNull(t1);
			Assert.NotNull(t2);
		}
	}
}