using System;
using System.IO;
using System.Linq;
using Core.Interfaces;
using Core.Services;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
	[TestFixture]
	[Category("ProcessService")]
	public class ProcessServiceTests : BaseTest
	{
		private IProcess _processService;
		private IDocuSignNotification _docuSignNotificationService;
		private DockyardAccount _userService;
		private string _testUserId = "testuser";
		private string _xmlPayloadFullPath;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_processService = ObjectFactory.GetInstance<IProcess>();
			_userService = ObjectFactory.GetInstance<DockyardAccount>();
			_docuSignNotificationService = ObjectFactory.GetInstance<IDocuSignNotification>();

			_xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory);
			if (_xmlPayloadFullPath == string.Empty)
				throw new Exception("XML payload file for testing DocuSign notification is not found.");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void ProcessService_ThrowsIfXmlInvalid()
		{
			_docuSignNotificationService.Process(_testUserId,
				File.ReadAllText(_xmlPayloadFullPath.Replace(".xml", "_invalid.xml")));
		}

		[Test]
		public void ProcessService_NotificationReceivedAlertCreated()
		{
			_docuSignNotificationService.Process(_testUserId, File.ReadAllText(_xmlPayloadFullPath));

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var fact = uow.FactRepository.GetAll().Where(f => f.Activity == "Received").SingleOrDefault();
				Assert.IsNotNull(fact);
			}
		}

		[Test]
		public void ProcessService_CanRetrieveValidProcesses()
		{
			//Arrange 
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				foreach (var p in FixtureData.GetProcesses())
				{
					uow.ProcessRepository.Add(p);
				}
				uow.SaveChanges();
			}

			//Act
			var processList = _userService.GetProcessList(_testUserId);

			//Assert
			Assert.AreEqual(2, processList.Count());
		}

		[Test]
		[Ignore("Seems like this test has no sense anymore due to the latest process changes")]
		public void ProcessService_CanCreateProcessProcessingAlert()
		{
			//Arrange 
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				foreach (var p in FixtureData.GetProcesses())
				{
					uow.ProcessRepository.Add(p);
				}
				uow.SaveChanges();
			}

			//Act
			_docuSignNotificationService.Process(_testUserId, File.ReadAllText(_xmlPayloadFullPath));

			//Assert
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var fact = uow.FactRepository.GetAll().Where(f => f.Activity == "Processed");
				Assert.AreEqual(2, fact.Count());
			}
		}

		[Test]
		public void ProcessService_Can_CreateProcess()
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var envelope = FixtureData.TestEnvelope1();
				var processTemplate = FixtureData.TestProcessTemplate1();

				uow.EnvelopeRepository.Add(envelope);
				uow.ProcessTemplateRepository.Add(processTemplate);
				uow.SaveChanges();

				var process = _processService.Create(processTemplate.Id, envelope.Id);
				Assert.IsNotNull(process);
				Assert.IsTrue(process.Id > 0);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ProcessService_CanNot_CreateProcessWithIncorrectEnvelope()
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				const int incorrectEnvelopeId = 2;

				var processTemplate = FixtureData.TestProcessTemplate1();

				uow.ProcessTemplateRepository.Add(processTemplate);
				uow.SaveChanges();
				_processService.Create(processTemplate.Id, incorrectEnvelopeId);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ProcessService_CanNot_CreateProcessWithIncorrectProcessTemplate()
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				const int incorrectProcessTemplateId = 2;

				var envelope = FixtureData.TestEnvelope1();

				uow.EnvelopeRepository.Add(envelope);
				uow.SaveChanges();
				_processService.Create(incorrectProcessTemplateId, envelope.Id);
			}
		}

		[Test]
		public void ProcessService_Can_ExecuteWithoutExceptions()
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var template = FixtureData.TestProcessTemplate1();
				var envelope = FixtureData.TestEnvelope1();

				uow.EnvelopeRepository.Add(envelope);
				uow.ProcessTemplateRepository.Add(template);
				uow.SaveChanges();

				_processService.Execute(template, envelope);
			}
		}
	}
}