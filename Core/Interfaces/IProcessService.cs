using Data.Entities;

namespace Core.Interfaces
{
	public interface IProcessService
	{
		void HandleDocusignNotification( string userId, string xmlPayload );
		ProcessDO Create( string processTemplateId, string envelopeId );
	}
}