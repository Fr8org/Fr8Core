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
	public class DocuSignRecipientTests : BaseTest
	{
		private readonly string TEMPLATE_WITH_ROLES_ID = "9a318240-3bee-475c-9721-370d1c22cec4";
		private IDocuSignRecipient _docusignRecipient;

		public DocuSignRecipientTests()
		{
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_docusignRecipient = ObjectFactory.GetInstance<IDocuSignRecipient>();
		}

		[Test]
		public void GetRecipients_ExistsTemplate_ShouldBeOk()
		{
			var recipientsDTO = _docusignRecipient.GetByTemplate(TEMPLATE_WITH_ROLES_ID);
			var recipients = recipientsDTO.Recipients;

			Assert.AreEqual(4, recipients.Count);
			Assert.NotNull(recipients.Where(x => x.Role == "Director").SingleOrDefault());
			Assert.NotNull(recipients.Where(x => x.Email == "reasyu@gmail.com").SingleOrDefault());

			Assert.NotNull(recipients.Where(x => x.Role == "President").SingleOrDefault());
			Assert.NotNull(recipients.Where(x => x.Email == "docusign_developer@dockyard.company").SingleOrDefault());

			Assert.NotNull(recipients.Where(x => x.Role == "Project Manager").SingleOrDefault());
			Assert.NotNull(recipients.Where(x => x.Email == "joanna@fogcitymail.com").SingleOrDefault());

			Assert.NotNull(recipients.Where(x => x.Role == "Vise President").SingleOrDefault());
			Assert.NotNull(recipients.Where(x => x.Email == "reasyu@yandex.ru").SingleOrDefault());
		}
		[Test]
		public void GetRecipients_NonExistsTemplate_ExpectedException()
		{
			var ex = Assert.Throws<InvalidOperationException>(() => _docusignRecipient.GetByTemplate(Guid.NewGuid().ToString()));
		}
	}
}