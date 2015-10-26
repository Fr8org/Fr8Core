using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

namespace Web.Controllers
{
	[RoutePrefix("webservices")]
	public class WebServicesController : ApiController
	{
		[HttpGet]
		[ResponseType(typeof(IEnumerable<WebServiceDTO>))]
		public IHttpActionResult Get()
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var models = uow.WebServiceRepository.GetAll()
					.Select(x => Mapper.Map<WebServiceDTO>(x))
					.ToList();

				return Ok(models);
			}
		}

		[HttpPost]
		public IHttpActionResult Post(string name, string icon)
		{
			return Ok();
		}
	}
}
