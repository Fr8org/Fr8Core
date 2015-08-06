using System.Collections.Generic;
using System.Web.Http;
using Web.Controllers.Services;
using Web.ViewModels;

namespace Web.Controllers
{
    public class ActionController : ApiController
    {
        private readonly IActionsService _service;

        public ActionController()
        {
            _service = new ActionsService();
        }

        public IEnumerable<ActionVM> Get()
        {
            return _service.GetAllActions();
        }

        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        public IEnumerable<ActionVM> Save(ActionVM actionVm)
        {
            if (_service.SaveOrUpdateAction(actionVm))
            {
                return new List<ActionVM> {actionVm};
            }
            return new List<ActionVM>();
        }
    }
}