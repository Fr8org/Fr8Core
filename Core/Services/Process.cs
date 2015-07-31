using System;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
	public class Process: IProcess
	{
		private readonly EventReporter _alertReporter;
		private readonly DockyardAccount _user;
		private readonly IDocusignXml _docusignXml;
		private readonly IProcessNodeService _processNode;

		public Process( EventReporter alertReporter, DockyardAccount userService, IDocusignXml docusignXml )
		{
			this._alertReporter = alertReporter;
			this._user = userService;
			this._docusignXml = docusignXml;
			this._processNode = ObjectFactory.GetInstance< IProcessNodeService >();
		}

		/// <summary>
		/// New Process object
		/// </summary>
		/// <param name="processTemplateId"></param>
		/// <param name="envelopeId"></param>
		/// <returns></returns>
		public ProcessDO Create( int processTemplateId, int envelopeId )
		{
			var curProcess = new ProcessDO();
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var template = uow.ProcessTemplateRepository.GetByKey( processTemplateId );
				var envelope = uow.EnvelopeRepository.GetByKey( envelopeId );

				if( template == null )
					throw new ArgumentNullException( "processTemplateId" );
				if( envelope == null )
					throw new ArgumentNullException( "envelopeId" );

				curProcess.Name = template.Name;
				curProcess.ProcessState = ProcessState.Processing;
				curProcess.EnvelopeId = envelopeId.ToString();

				var processNode = this._processNode.Create( uow, curProcess );
				uow.SaveChanges();

				curProcess.CurrentProcessNodeId = processNode.Id;

				uow.ProcessRepository.Add( curProcess );
				uow.SaveChanges();
			}
			return curProcess;
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

			var processList = this._user.GetProcessList( userId );
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