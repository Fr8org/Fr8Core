using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Services;
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ActionListController: ApiController
	{
        /// <summary>
        /// Retrieve ActionList by specifying Subroute.Id and ActionListType.
        /// </summary>
        /// <param name="id">Subroute.Id</param>
        /// <param name="actionListType">ActionListType</param>
        /// <returns></returns>
        //[ResponseType(typeof(ActionListDTO))]
        [ActionName("bySubroute")]
        [HttpGet]
        public IHttpActionResult GetBySubrouteId(int id, int actionListType)
        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                var curActionList = uow..GetQuery()
//                    .SingleOrDefault(x => x.SubrouteID == id && x.ActionListType == actionListType);
//
//                return Ok(Mapper.Map<ActionListDTO>(curActionList));
//            }
            return NotFound();
        }

		/*public IEnumerable<ActionListDO> GetAll()
		{
            return _actionList.GetAll();
		}*/

        public IHttpActionResult GetAll()
		{
            return NotFound();
		}

//        public ActionListDO Get(int id)
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                return _actionList.GetByKey(uow,id);
//            }
//            
//        }

        public IHttpActionResult Get(int id)
        {
            return NotFound();
        }
	}
}