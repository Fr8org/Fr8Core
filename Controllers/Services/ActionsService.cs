using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers.Services
{
	public interface IActionsService
	{
		IEnumerable< ActionVM > GetAllActions();
		IEnumerable< ActionListVM > GetAllActionLists();
	    bool SaveOrUpdateAction(ActionVM action);
	}

	public class ActionsService: IActionsService
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

		public IEnumerable< ActionListVM > GetAllActionLists()
		{
			var items = new List< ActionListVM >();

			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var actionLists = uow.ActionListRepository.GetAll();
				items.AddRange( actionLists.Select( Mapper.Map< ActionListVM > ) );
			}

			return items;
		}

	    public bool SaveOrUpdateAction(ActionVM action)
	    {
	        using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
	        {
                var actionDo = Mapper.Map<ActionDO>(action);
	            var existingAction = uow.ActionRepository.GetByKey(action.Id);
	            if (existingAction != null)
	            {
                    existingAction.ActionList = actionDo.ActionList;
                    existingAction.ActionListId = actionDo.ActionListId;
                    existingAction.ActionType = actionDo.ActionType;
                    existingAction.ConfigurationSettings = actionDo.ConfigurationSettings;
                    existingAction.FieldMappingSettings = actionDo.FieldMappingSettings;
                    existingAction.ParentPluginRegistration = actionDo.ParentPluginRegistration;
                    existingAction.UserLabel = actionDo.UserLabel;
	            }
	            else
	            {
                    uow.ActionRepository.Add(actionDo);
	            }

	            try
	            {
                    uow.SaveChanges();
                    return true;
	            }
	            catch (Exception)
	            {
	                return false;
	            }
	        }
	    }
	}
}