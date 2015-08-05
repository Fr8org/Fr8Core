using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Managers;
using Core.Utilities;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
	public class DocuSignNotification: IDocuSignNotification
	{
		private readonly EventReporter _alertReporter;
		private readonly DockyardAccount _user;
		private readonly IProcessTemplate _processTemplate;

		public DocuSignNotification( EventReporter alertReporter, DockyardAccount userService )
		{
			this._alertReporter = alertReporter;
			this._user = userService;
			this._processTemplate = ObjectFactory.GetInstance< ProcessTemplate >();
		}

		/// <summary>
		/// The method processes incoming notifications from DocuSign. 
		/// </summary>
		/// <param name="userId">UserId received from DocuSign.</param>
		/// <param name="xmlPayload">XML content received from DocuSign.</param>
		public void Process( string userId, string xmlPayload )
		{
			if( string.IsNullOrEmpty( userId ) )
				throw new ArgumentNullException( "userId" );

			if( string.IsNullOrEmpty( xmlPayload ) )
				throw new ArgumentNullException( "xmlPayload" );

			List< DocuSignEventDO > curEvents;
			string curEnvelopeId;
			this.Parse( xmlPayload, out curEvents, out curEnvelopeId );

			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				foreach( var curEvent in curEvents )
				{
					var @event = curEvent;
					var subscriptions = uow.ExternalEventRegistrationRepository.GetQuery().Where( s => s.EventType == @event.ExternalEventType ).ToList();
					var envelope = uow.EnvelopeRepository.GetByKey( curEvent.Id );

					foreach( var subscription in subscriptions )
					{
						this._processTemplate.LaunchProcess( subscription.ProcessTemplateId.Value, envelope );
					}
				}
			}

			this._alertReporter.DocusignNotificationReceived( userId, curEnvelopeId);
			this.HandleIncomingNotification( userId, curEnvelopeId );
		}

		private void Parse( string xmlPayload, out List< DocuSignEventDO > curEvents, out string curEnvelopeId )
		{
			curEvents = new List< DocuSignEventDO >();;
			try
			{
				var docuSignEnvelopeInformation = DocuSignConnectParser.GetEnvelopeInformation( xmlPayload );
				curEnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId;
				curEvents.Add( new DocuSignEventDO
				{
					ExternalEventType = ExternalEventType.MapEnvelopeExternalEventType(docuSignEnvelopeInformation.EnvelopeStatus.Status),
					EnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId
				} );
			}
			catch( ArgumentException )
			{
				const string message = "Cannot extract envelopeId from DocuSign notification: UserId {0}, XML Payload\r\n{1}";
				this._alertReporter.ImproperDocusignNotificationReceived( message );
				throw new ArgumentException();
			}
		}

		// <summary>
		/// Handles a notification by DocuSign by an individual Process.
		/// </summary>
		/// <param name="userId">UserId received from DocuSign.</param>
		/// <param name="envelopeId">EnvelopeId received from DocuSign.</param>
		private void HandleIncomingNotification( string userId, string envelopeId )
		{
			var processList = this._user.GetProcessList( userId );
			foreach( var process in processList )
			{
				this._alertReporter.AlertProcessProcessing( userId, envelopeId, process.Id );
				//TODO: all notification processing logic.
			}
		}
	}
}