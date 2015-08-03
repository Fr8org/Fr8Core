using Data.Entities;
using Data.States;

namespace Core.Interfaces
{
	public interface IProcessTemplate
	{
		void CreateOrUpdate( ProcessTemplateDO ptdo );
		void Delete( int id );
		void HandleExternalEvent( ExternalEventType curEventType );
	}
}