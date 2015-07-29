using System.Collections.Generic;
using System.Web.Http;
using Core.Interfaces;
using StructureMap;
using Web.Controllers.Services;
using Web.ViewModels;

namespace Web.Controllers
{
	public class ActionListController: ApiController
	{
		private readonly IActionsService _service;

		private ActionListController()
		{
			this._service = new ActionsService(ObjectFactory.GetInstance<ISubscriptionService>());
		}

		public IEnumerable< ActionListVM > Get()
		{
			return this._service.GetAllActionLists();
		}
	}
}