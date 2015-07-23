using System.Collections.Generic;
using System.Web.Http;
using Web.Controllers.Services;
using Web.ViewModels;

namespace Web.Controllers
{
	public class ActionListController: ApiController
	{
		private readonly IActionsService _service;

		private ActionListController()
		{
			this._service = new ActionsService();
		}

		public IEnumerable< ActionListVM > Get()
		{
			return this._service.GetAllActionLists();
		}
	}
}