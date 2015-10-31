using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Data.Entities;

namespace Web.Controllers
{
	[RoutePrefix("webservices")]
	public class WebServicesController : ApiController
	{
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

		[ResponseType(typeof(WebServiceDTO))]
		public IHttpActionResult Post(WebServiceDTO webService)
		{
			WebServiceDO entity = Mapper.Map<WebServiceDO>(webService);

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.WebServiceRepository.Add(entity);

				uow.SaveChanges();
			}

			var model = Mapper.Map<WebServiceDTO>(entity);

			return Ok(model);
		}
	}
}
