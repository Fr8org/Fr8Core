using Data.Entities;

namespace Core.Interfaces
{
	public interface IProcessTemplate
	{
		void CreateOrUpdate( ProcessTemplateDO ptdo );
		void Delete( int id );
		void LaunchProcess( int curProcessTemplateId, EnvelopeDO curEnvelope );
	}
}