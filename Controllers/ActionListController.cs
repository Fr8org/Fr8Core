using System.Collections.Generic;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
	public class ActionListController: ApiController
	{
		private readonly IAction _action;

		private ActionListController()
		{
			this._action = new Action();
		}

		public IEnumerable<ActionListVM> Get()
		{
            return this._action.GetAllActionLists<ActionListVM>();
		}
	}
}