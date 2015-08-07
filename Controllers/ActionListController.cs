using Core.Interfaces;
using Core.Services;
using Data.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Web.ViewModels;

namespace Web.Controllers
{
	public class ActionListController: ApiController
	{
		private readonly IActionList _service;

        public ActionListController()
		{
			this._service = new ActionList();
		}

		public IEnumerable<ActionListDO> GetAll()
		{
			return this._service.GetAll();
		}

        public ActionListDO Get(int id)
        {
            return this._service.Get(id);
        }

        public bool AddAction(ActionDO curActionDO, string position)
        {
            return this._service.AddAction(curActionDO, position);
        }

        public void Process(ActionListDO curActionListDO)
        {
            this._service.Process(curActionListDO);
        }
	}
}