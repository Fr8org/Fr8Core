using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Core.Interfaces;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Web.ViewModels;

namespace Web.Controllers
{
    [RoutePrefix("api/actionList")]
	public class ActionListController: ApiController
	{
        private readonly IActionList _actionList;

        public ActionListController()
		{
            _actionList = new ActionList();
		}

        /// <summary>
        /// Retrieve ActionList by specifying ProcessNodeTemplate.Id and ActionListType.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate.Id</param>
        /// <param name="actionListType">ActionListType</param>
        /// <returns></returns>
        [ResponseType(typeof(ActionListDTO))]
        [Route("byProcessNodeTemplate")]
        [HttpGet]
        public IHttpActionResult GetByProcessNodeTemplateId(int id, int actionListType)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionList = uow.ActionListRepository.GetQuery()
                    .SingleOrDefault(x => x.ProcessNodeTemplateID == id && x.ActionListType == actionListType);

                return Ok(Mapper.Map<ActionListDTO>(curActionList));
            }
        }

		public IEnumerable<ActionListDO> GetAll()
		{
            return _actionList.GetAll();
		}

        public ActionListDO Get(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return _actionList.GetByKey(uow,id);
            }
            
        }
	}
}