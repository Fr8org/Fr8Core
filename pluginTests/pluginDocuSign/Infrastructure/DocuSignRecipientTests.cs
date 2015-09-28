using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;

using DocuSign.Integrations.Client;

using NUnit.Framework;
using Utilities;

using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;
using Data.Migrations;
using Newtonsoft.Json;
using StructureMap;
using pluginDocuSign.Services;
using pluginDocuSign.Interfaces;
using pluginDocuSign;

namespace DockyardTest.DocuSign
{
	[TestFixture]
	public class DocuSignRecipientTests : BaseTest
	{
		private readonly string TEMPLATE_WITH_ROLES_ID = "9a318240-3bee-475c-9721-370d1c22cec4";

		public DocuSignRecipientTests()
		{
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			PluginDocuSignMapBootstrapper.ConfigureDependencies(PluginDocuSignMapBootstrapper.DependencyType.LIVE);
		}

		[Test]
		public void GetRecipients_ExistsTemplate_ShouldBeOk()
		{
			DocuSignRecipient docusignRecipient = new DocuSignRecipient();
			var recipientsDTO = docusignRecipient.GetByTemplate(TEMPLATE_WITH_ROLES_ID);
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
			DocuSignRecipient docusignRecipient = new DocuSignRecipient();

			var ex = Assert.Throws<InvalidOperationException>(() => docusignRecipient.GetByTemplate(Guid.NewGuid().ToString()));
		}
	}
}