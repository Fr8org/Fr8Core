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
        private readonly IActionList _actionList;

        public ActionListController()
		{
            _actionList = new ActionList();
		}

		public IEnumerable<ActionListDO> GetAll()
		{
            return _actionList.GetAll();
		}

        public ActionListDO Get(int id)
        {
            return _actionList.GetByKey(id);
        }
	}
}