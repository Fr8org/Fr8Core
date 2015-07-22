using System.Collections.Generic;
using System.Web.Http;
using Web.Controllers.Services;
using Web.ViewModels;

namespace Web.Controllers
{
	public class ActionsController: ApiController
	{
		private readonly IActionsService _service;

		private ActionsController()
		{
			this._service = new ActionsService();
		}

		public IEnumerable< ActionVM > Get()
		{
			return this._service.GetAllActions();
		}
	}
}