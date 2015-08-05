using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
	public class ProcessTemplate: IProcessTemplate
	{
		private readonly IProcess _process;

		public ProcessTemplate()
		{
			this._process = ObjectFactory.GetInstance< IProcess >();
		}

		public IQueryable< ProcessTemplateDO > GetForUser( string userId, int? id = null )
		{
			using( var unitOfWork = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				return unitOfWork.ProcessTemplateRepository
					.GetQuery()
					.Where( pt => pt.UserId == userId || ( id != null && pt.Id == id ) );
			}
		}

		public int CreateOrUpdate( ProcessTemplateDO ptdo )
		{
			var creating = ptdo.Id == 0;

			using( var unitOfWork = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				if( creating )
				{
					unitOfWork.ProcessTemplateRepository.Add( ptdo );
				}
				else
				{
					var curProcessTemplate = unitOfWork.ProcessTemplateRepository.GetByKey( ptdo.Id );
					if( curProcessTemplate == null )
						throw new EntityNotFoundException();
					curProcessTemplate.Name = ptdo.Name;
					curProcessTemplate.Description = ptdo.Description;
				}
				unitOfWork.SaveChanges();
			}

			return ptdo.Id;
		}

		public void Delete( int id )
		{
			using( var unitOfWork = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var curProcessTemplate = unitOfWork.ProcessTemplateRepository.GetByKey( id );
				if( curProcessTemplate == null )
				{
					throw new EntityNotFoundException< ProcessTemplateDO >( id );
				}
				unitOfWork.ProcessTemplateRepository.Remove( curProcessTemplate );
				unitOfWork.SaveChanges();
			}
		}

		public void LaunchProcess( int curProcessTemplateId, EnvelopeDO curEnvelope )
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey( curProcessTemplateId );
				if( curProcessTemplate.ProcessTemplateState != ProcessTemplateState.Inactive )
				{
					this._process.Execute( curProcessTemplate, curEnvelope );
				}
			}
		}
	}
}