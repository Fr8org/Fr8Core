using System.Linq;
using Core.Exceptions;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
	public class ProcessTemplate: IProcessTemplate
	{
		private EventReporter _eventReporter;

		public ProcessTemplate()
		{
			this._eventReporter = ObjectFactory.GetInstance< EventReporter >();
		}

		public void Delete( int id )
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var ptdo = uow.ProcessTemplateRepository.GetByKey( id );
				if( ptdo == null )
				{
					throw new EntityNotFoundException();
				}
				uow.ProcessTemplateRepository.Remove( ptdo );
				uow.SaveChanges();
			}

		}

		public void HandleExternalEvent( ExternalEventType curEventType )
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var externalEventRegistrations = uow.ExternalEventRegistrationRepository.GetQuery().Where( e => e.EventType.Equals( curEventType ) );
				foreach( var registration in externalEventRegistrations )
				{
					if( registration.ProcessTemplateId != null )
						this.LaunchProcess( registration.ProcessTemplateId.Value );
				}
			}
		}

		public void CreateOrUpdate( ProcessTemplateDO ptdo )
		{
			var creating = ptdo.Id == 0;

			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				if( ptdo.Id == 0 )
				{
					uow.ProcessTemplateRepository.Add( ptdo );
				}
				else
				{
					var entity = uow.ProcessTemplateRepository.GetByKey( ptdo.Id );

					if( entity == null )
						throw new EntityNotFoundException();

					entity.Name = ptdo.Name;
					entity.Description = ptdo.Description;
				}
				uow.SaveChanges();
			}

			//if (creating)
			//{
			//    _eventReporter.ProcessTemplateCreated(ptdo.UserId, ptdo.Name);
			//}
		}

		private void LaunchProcess( int curProcessTemplateId )
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey( curProcessTemplateId );
				if( curProcessTemplate.ProcessTemplateState != ProcessTemplateState.Inactive )
				{
				}
			}
		}
	}
}