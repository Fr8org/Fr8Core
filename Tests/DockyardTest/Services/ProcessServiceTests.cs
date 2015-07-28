using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Interfaces;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
	[ TestFixture ]
	[ Category( "ProcessService" ) ]
	[ Ignore( "Tests do not pass on CI." ) ]
	public class ProcessServiceTests: BaseTest
	{
		private IProcessService _processService;
		private DockyardAccount _userService;
		private IDocusignXml _docusignXml;
		private string _testUserId = "testuser";
		private string _xmlPayloadFullPath;

		[ SetUp ]
		public override void SetUp()
		{
			base.SetUp();
			_processService = ObjectFactory.GetInstance< IProcessService >();
			_userService = ObjectFactory.GetInstance< DockyardAccount >();
			_docusignXml = ObjectFactory.GetInstance< IDocusignXml >();

			_xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath( Environment.CurrentDirectory );
			if( _xmlPayloadFullPath == string.Empty )
				throw new Exception( "XML payload file for testing DocuSign notification is not found." );
		}

		[ Test ]
		public void ProcessService_CanExtractEnvelopeData()
		{
			string envelopeId = _docusignXml.GetEnvelopeIdFromXml( File.ReadAllText( _xmlPayloadFullPath ) );
			Assert.AreEqual( "0aa561b8-b4d9-47e0-a615-2367971f876b", envelopeId );
		}

		[ Test ]
		[ ExpectedException( typeof( ArgumentException ) ) ]
		public void ProcessService_ThrowsIfXmlInvalid()
		{
			_processService.HandleDocusignNotification( _testUserId, File.ReadAllText( _xmlPayloadFullPath.Replace( ".xml", "_invalid.xml" ) ) );
		}

		[ Test ]
		public void ProcessService_NotificationReceivedAlertCreated()
		{
			_processService.HandleDocusignNotification( _testUserId, File.ReadAllText( _xmlPayloadFullPath ) );

			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				FactDO fact = uow.FactRepository.GetAll().Where( f => f.Activity == "Received" ).SingleOrDefault();
				Assert.IsNotNull( fact );
			}
		}

		[ Test ]
		public void ProcessService_CanRetrieveValidProcesses()
		{
			//Arrange 
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				foreach( ProcessDO p in FixtureData.GetProcesses() )
				{
					uow.ProcessRepository.Add( p );
				}
				uow.SaveChanges();
			}

			//Act
			var processList = _userService.GetProcessList( _testUserId );

			//Assert
			Assert.AreEqual( 2, processList.Count() );
		}

		[ Test ]
		public void ProcessService_CanCreateProcessProcessingAlert()
		{
			//Arrange 
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				foreach( ProcessDO p in FixtureData.GetProcesses() )
				{
					uow.ProcessRepository.Add( p );
				}
				uow.SaveChanges();
			}

			//Act
			_processService.HandleDocusignNotification( _testUserId, File.ReadAllText( _xmlPayloadFullPath ) );

			//Assert
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				IEnumerable< FactDO > fact = uow.FactRepository.GetAll().Where( f => f.Activity == "Processed" );
				Assert.AreEqual( 2, fact.Count() );
			}
		}

		[ Test ]
		public void ProcessService_Can_CreateProcess()
		{
			const string processTemplateId = "1";
			const string envelopeId = "2";
			var process = this._processService.Create( processTemplateId, envelopeId );

			Assert.IsNotNull( process );
			Assert.IsTrue( process.Id > 0 );
		}
	}
}