using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Interfaces;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers.Services
{
	public interface IActionsService
	{
		IEnumerable< ActionVM > GetAllActions();
	}

	public class ActionsService:IActionsService
	{
		public IEnumerable< ActionVM > GetAllActions()
		{
			var items = new List< ActionVM >();

			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var actions = uow.ActionRepository.GetAll();
				items.AddRange( actions.Select( Mapper.Map< ActionVM > ) );
			}

			return items;
		}
	}
}