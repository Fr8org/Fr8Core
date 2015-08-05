using System;
using System.Threading.Tasks;
using System.Web.Http;
using Core.Interfaces;
using Core.Managers;
using StructureMap;

namespace Web.Controllers
{
	public class DocuSignNotificationController : ApiController
	{
		private readonly IDocuSignNotification _docuSignNotificationService;
		private readonly EventReporter _alertReporter;

		public DocuSignNotificationController()
		{
			this._docuSignNotificationService = ObjectFactory.GetInstance< IDocuSignNotification >();
			this._alertReporter = ObjectFactory.GetInstance< EventReporter >();
		}

		public DocuSignNotificationController( IDocuSignNotification docusignNotificationService )
		{
			this._docuSignNotificationService = docusignNotificationService;
			this._alertReporter = ObjectFactory.GetInstance< EventReporter >();
		}

		/// <summary>
		/// Processes incoming DocuSign notifications.
		/// </summary>
		/// <returns>HTTP 200 if notification is successfully processed, 
		/// HTTP 400 if request does not contain all expected data or malformed.</returns>
		[ HttpPost ]
		public async Task< IHttpActionResult > HandleDocuSignNotification( [ FromUri ] string userId )
		{
			var xmlPayload = await this.Request.Content.ReadAsStringAsync();

			if( string.IsNullOrEmpty( userId ) )
			{
				var message = "Cannot find userId in DocuSign notification. XML payload";
				this._alertReporter.ImproperDocusignNotificationReceived( message );
				return this.BadRequest( message );
			}

			if( string.IsNullOrEmpty( xmlPayload ) )
			{
				var message = string.Format( "Cannot find XML payload in DocuSign notification: UserId {0}.",
					userId );
				this._alertReporter.ImproperDocusignNotificationReceived( message );
				return this.BadRequest( message );
			}

			try
			{
				this._docuSignNotificationService.Process( userId, xmlPayload );
			}
			catch( ArgumentException )
			{
				//The event is already logged.
				return this.BadRequest( "Cannot find envelopeId in XML payload." );
			}
			return this.Ok();
		}

		public void Get()
		{
			throw new Exception();
		}
	}
}