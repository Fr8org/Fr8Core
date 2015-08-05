using System;
using System.IO;
using System.Linq;
using Core.Interfaces;
using Core.Services;
using Core.Utilities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
	[ TestFixture ]
	[ Category( "ProcessService" ) ]
	public class ProcessServiceTests: BaseTest
	{
		private IProcess _processService;
		private IDocuSignNotification _docuSignNotificationService;
		private DockyardAccount _userService;
		private string _testUserId = "testuser";
		private string _xmlPayloadFullPath;

		[ SetUp ]
		public override void SetUp()
		{
			base.SetUp();
			this._processService = ObjectFactory.GetInstance< IProcess >();
			this._userService = ObjectFactory.GetInstance< DockyardAccount >();
			this._docuSignNotificationService = ObjectFactory.GetInstance< IDocuSignNotification >();

			this._xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath( Environment.CurrentDirectory );
			if( this._xmlPayloadFullPath == string.Empty )
				throw new Exception( "XML payload file for testing DocuSign notification is not found." );
		}

		[ Test ]
		[ ExpectedException( typeof( ArgumentException ) ) ]
		public void ProcessService_ThrowsIfXmlInvalid()
		{
			this._docuSignNotificationService.Process( this._testUserId, File.ReadAllText( this._xmlPayloadFullPath.Replace( ".xml", "_invalid.xml" ) ) );
		}

		[ Test ]
		public void ProcessService_NotificationReceivedAlertCreated()
		{
			this._docuSignNotificationService.Process( this._testUserId, File.ReadAllText( this._xmlPayloadFullPath ) );

			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var fact = uow.FactRepository.GetAll().Where( f => f.Activity == "Received" ).SingleOrDefault();
				Assert.IsNotNull( fact );
			}
		}

		[ Test ]
		public void ProcessService_CanRetrieveValidProcesses()
		{
			//Arrange 
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				foreach( var p in FixtureData.GetProcesses() )
				{
					uow.ProcessRepository.Add( p );
				}
				uow.SaveChanges();
			}

			//Act
			var processList = this._userService.GetProcessList( this._testUserId );

			//Assert
			Assert.AreEqual( 2, processList.Count() );
		}

		[ Test ]
		public void ProcessService_CanCreateProcessProcessingAlert()
		{
			//Arrange 
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				foreach( var p in FixtureData.GetProcesses() )
				{
					uow.ProcessRepository.Add( p );
				}
				uow.SaveChanges();
			}

			//Act
			this._docuSignNotificationService.Process( this._testUserId, File.ReadAllText( this._xmlPayloadFullPath ) );

			//Assert
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var fact = uow.FactRepository.GetAll().Where( f => f.Activity == "Processed" );
				Assert.AreEqual( 2, fact.Count() );
			}
		}

		[ Test ]
		public void ProcessService_Can_CreateProcess()
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var envelope = FixtureData.CreateEnvelope();
				var processTemplate = FixtureData.CreateProcessTemplate();

				uow.EnvelopeRepository.Add( envelope );
				uow.ProcessTemplateRepository.Add( processTemplate );
				uow.SaveChanges();

				var process = this._processService.Create( processTemplate.Id, envelope.Id );
				Assert.IsNotNull( process );
				Assert.IsTrue( process.Id > 0 );
			}
		}

		[ Test ]
		[ ExpectedException( typeof( ArgumentNullException ) ) ]
		public void ProcessService_CanNot_CreateProcessWithIncorrectEnvelope()
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				const int incorrectEnvelopeId = 2;

				var processTemplate = FixtureData.CreateProcessTemplate();

				uow.ProcessTemplateRepository.Add( processTemplate );
				uow.SaveChanges();
				this._processService.Create( processTemplate.Id, incorrectEnvelopeId );
			}
		}

		[ Test ]
		[ ExpectedException( typeof( ArgumentNullException ) ) ]
		public void ProcessService_CanNot_CreateProcessWithIncorrectProcessTemplate()
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				const int incorrectProcessTemplateId = 2;

				var envelope = FixtureData.CreateEnvelope();

				uow.EnvelopeRepository.Add( envelope );
				uow.SaveChanges();
				this._processService.Create( incorrectProcessTemplateId, envelope.Id );
			}
		}
	}
}