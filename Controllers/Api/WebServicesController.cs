using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Hub.Interfaces;
using StructureMap;
using Hub.Services;

namespace HubWeb.Controllers
{
    public class WebServicesController : ApiController
    {
        private const string UknownWebServiceName = "UnknownService";
        private readonly IActivityTemplate _activityTemplate;

        public WebServicesController()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        [HttpGet]
        public IHttpActionResult Get(int id = -1)
        {
            if (id >= 0)
            {
                var category = (ActivityCategory)id;
                List<WebServiceActivitySetDTO> webServiceList;

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    // Getting web services and their actions as one set, then filtering that set
                    // to get only those actions whose category matches any of categories provided
                    // resulting set is grouped into batches 1 x web service - n x actions

                    var unknwonService = uow.WebServiceRepository.GetQuery().FirstOrDefault(x => x.Name == UknownWebServiceName);
                    Fr8Account fr8Account = new Fr8Account();

                    var activityTemplate = _activityTemplate.GetQuery()
                        .Where(x => x.ActivityTemplateState == ActivityTemplateState.Active)
                        .Where(x => id == 0 || category == x.Category)
                        .Where(x => x.Tags == null || !x.Tags.Contains(Tags.Internal));

                    if (!fr8Account.IsCurrentUserInAdminRole())
                        activityTemplate = activityTemplate.Where(x => x.ClientVisibility != false);

                    webServiceList = activityTemplate
                    .GroupBy(x => x.WebService, x => x, (key, group) => new
                    {
                        WebService = key,
                        SortOrder = key == null ? 1 : 0,
                        Actions = group
                    }).OrderBy(x => x.SortOrder)
                    .Select(x => new WebServiceActivitySetDTO
                    {
                        WebServiceIconPath = x.WebService != null ? x.WebService.IconPath : (unknwonService != null ? unknwonService.IconPath : null),
                        WebServiceName = x.WebService != null ? x.WebService.Name : string.Empty,
                        Activities = x.Actions
                         .GroupBy(y => y.Name)
                         .Select(y => y.OrderByDescending(z => int.Parse(z.Version)).First())
                         .Select(p => new ActivityTemplateDTO
                         {
                             Id = p.Id,
                             Name = p.Name,
                             Category = p.Category,
                             Label = p.Label,
                             MinPaneWidth = p.MinPaneWidth,
                             Version = p.Version,
                             Type = p.Type,
                             Tags = p.Tags,
                             WebService = Mapper.Map<WebServiceDTO>(p.WebService),
                             Terminal = Mapper.Map<TerminalDTO>(p.Terminal)
                         }).ToList()
                    }).ToList();
                }

                return Ok(webServiceList);
            }

            // If there is no category
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var models = uow.WebServiceRepository
                    .GetAll()
                    .Select(Mapper.Map<WebServiceDTO>)
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
    }
}