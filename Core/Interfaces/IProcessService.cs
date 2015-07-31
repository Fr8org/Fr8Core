using Data.Entities;

namespace Core.Interfaces
{
	public interface IProcess
	{
		void HandleDocusignNotification( string userId, string xmlPayload );
		ProcessDO Create( int processTemplateId, int envelopeId );
	}
}