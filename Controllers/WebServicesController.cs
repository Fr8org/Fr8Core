using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using StructureMap;

namespace HubWeb.Controllers
{
	[RoutePrefix("api/webservices")]
	public class WebServicesController : ApiController
	{
		[HttpGet]
		[Route("")]
		public IHttpActionResult GetWebServices()
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
		[Route("")]
		public IHttpActionResult CreateWebService(WebServiceDTO webService)
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

		[HttpPost]
		[Route("actions")]
		public IHttpActionResult GetActions(ActivityCategory[] categories)
		{
			var models = new List<WebServiceActionSetDTO>();

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				// Getting web services and their actions as one set, then filtering that set
				// to get only those actions whose category matches any of categories provided
				// resulting set is grouped into batches 1 x web service - n x actions
				models = uow.WebServiceRepository.GetQuery()
						.Join(uow.ActivityTemplateRepository.GetQuery(), ws => ws.Id, at => at.WebServiceId, (ws, at) => new { ws, at })
						.Where(x => categories.Any(p => p == x.at.Category))
						.GroupBy(x => x.ws, x => x.at, (key, group) => new
						{
							WebService = key,
							Actions = group.ToList()
						})
						.Select(x => new WebServiceActionSetDTO
						{
							WebServiceIconPath = x.WebService.IconPath,
							Actions = x.Actions.Select(p => new ActivityTemplateDTO
							{
								Name = p.Name
							})
							.ToList()
						})
						.ToList();
			}

			return Ok(models);
		}
	}
}