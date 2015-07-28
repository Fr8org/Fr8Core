using System;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;

namespace Core.Services
{
	public class ProcessService: IProcessService
	{
		private readonly EventReporter _alertReporter;
		private readonly DockyardAccount _userService;
		private readonly IDocusignXml _docusignXml;

		public ProcessService( EventReporter alertReporter, DockyardAccount userService, IDocusignXml docusignXml )
		{
			this._alertReporter = alertReporter;
			this._userService = userService;
			this._docusignXml = docusignXml;
		}

		/// <summary>
		/// New Process object
		/// </summary>
		/// <param name="processTemplateId"></param>
		/// <param name="envelopeId"></param>
		/// <returns></returns>
		public ProcessDO Create( string processTemplateId, string envelopeId )
		{
			return null;
		}

		/// <summary>
		/// The method processes incoming notifications from DocuSign. 
		/// </summary>
		/// <param name="userId">UserId received from DocuSign.</param>
		/// <param name="xmlPayload">XML content received from DocuSign.</param>
		public void HandleDocusignNotification( string userId, string xmlPayload )
		{
			string envelopeId;

			if( string.IsNullOrEmpty( userId ) )
				throw new ArgumentNullException( "userId" );

			if( string.IsNullOrEmpty( xmlPayload ) )
				throw new ArgumentNullException( "xmlPayload" );

			try
			{
				envelopeId = this._docusignXml.GetEnvelopeIdFromXml( xmlPayload );
			}
			catch( ArgumentException )
			{
				const string message = "Cannot extract envelopeId from DocuSign notification: UserId {0}, XML Payload\r\n{1}";
				this._alertReporter.ImproperDocusignNotificationReceived( message );
				throw new ArgumentException();
			}

			this._alertReporter.DocusignNotificationReceived( userId, xmlPayload );

			var processList = this._userService.GetProcessList( userId );
			foreach( var process in processList )
			{
				this.HandleIncomingNotification( userId, envelopeId, process );
			}
		}

		/// <summary>
		/// Handles a notification by DocuSign by an individual Process.
		/// </summary>
		/// <param name="userId">UserId received from DocuSign.</param>
		/// <param name="envelopeId">EnvelopeId received from DocuSign.</param>
		/// <param name="process">An instance of ProcessDO object for which processing should occur.</param>
		private void HandleIncomingNotification( string userId, string envelopeId, ProcessDO process )
		{
			this._alertReporter.AlertProcessProcessing( userId, envelopeId, process.Id );
			//TODO: all notification processing logic.
		}

	}
}