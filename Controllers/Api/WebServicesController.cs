using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using StructureMap;

namespace HubWeb.Controllers
{
	public class WebServicesController : ApiController
	{
	    private const string UknownWebServiceName = "UnknownService";
	    private IActivityTemplate _activityTemplate;

	    public WebServicesController()
	    {
	        _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
	    }

	    [HttpGet]
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

		[HttpPost]
		[ActionName("actions")]
		public IHttpActionResult GetActions(ActivityCategory[] categories)
		{
			List<WebServiceActionSetDTO> webServiceList;

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				// Getting web services and their actions as one set, then filtering that set
				// to get only those actions whose category matches any of categories provided
				// resulting set is grouped into batches 1 x web service - n x actions

                var templates = _activityTemplate.GetAll();
                var unknwonService = uow.WebServiceRepository.GetQuery().FirstOrDefault(x => x.Name == UknownWebServiceName);

			    webServiceList = templates
                    .Where(x=> x.ActivityTemplateState == ActivityTemplateState.Active)
                    .Where(x => categories == null || categories.Contains(x.Category))
			        .GroupBy(x => x.WebService, x => x, (key, group) => new
			        {
			            WebService = key,
			            SortOrder = key == null ? 1 : 0,
			            Actions = group
			        }).OrderBy(x => x.SortOrder)
			        .Select(x => new WebServiceActionSetDTO
			        {
			            WebServiceIconPath = x.WebService != null ? x.WebService.IconPath : (unknwonService != null ? unknwonService.IconPath : null),
			            Actions = x.Actions.Select(p => new ActivityTemplateDTO
			            {
			                Id = p.Id,
			                Name = p.Name,
			                Category = p.Category,
			                ComponentActivities = p.ComponentActivities,
			                Label = p.Label,
			                MinPaneWidth = p.MinPaneWidth,
			                TerminalId = p.Terminal.Id,
			                Version = p.Version,
                            Type = p.Type
			            }).ToList()
			        }).ToList();
			}

			return Ok(webServiceList);
		}
	}
}