using System.Web.Http;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ActionListController: ApiController
	{
        /// <summary>
        /// Retrieve ActionList by specifying SubPlan.Id and ActionListType.
        /// </summary>
        /// <param name="id">SubPlan.Id</param>
        /// <param name="actionListType">ActionListType</param>
        /// <returns></returns>
        //[ResponseType(typeof(ActionListDTO))]
        [ActionName("bySubPlan")]
        [HttpGet]
        public IHttpActionResult GetBySubPlanId(int id, int actionListType)
        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                var curActionList = uow..GetQuery()
//                    .SingleOrDefault(x => x.SubPlanID == id && x.ActionListType == actionListType);
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