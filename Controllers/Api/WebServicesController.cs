using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    public class WebServicesController : ApiController
    {
        private const string UknownWebServiceName = "UnknownService";
        private readonly IActivityTemplate _activityTemplate;
        private readonly Fr8Account _fr8Account;

        public WebServicesController()
        {
            _fr8Account = ObjectFactory.GetInstance<Fr8Account>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        /// <summary>
        /// Retrieves collection of web services which contain activities of specified category. If category is not specified returns list of web servies only
        /// </summary>
        /// <param name="id">Id of activity category. 1 - Monitors, 2 - Receivers, 3 - Processors, 4 - Forwarders, 5 - Solutions</param>
        // [HttpGet]
        // [SwaggerResponse(HttpStatusCode.OK, "Collection of web services including activity templates", typeof(List<WebServiceActivitySetDTO>))]
        // public IHttpActionResult Get(int id = -1)
        // {
        //     if (id >= 0)
        //     {
        //         var category = (Fr8.Infrastructure.Data.States.ActivityCategory)id;
        //         List<WebServiceActivitySetDTO> webServiceList;
        // 
        //         using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //         {
        //             // Getting web services and their actions as one set, then filtering that set
        //             // to get only those actions whose category matches any of categories provided
        //             // resulting set is grouped into batches 1 x web service - n x actions
        // 
        //             var unknwonService = uow.WebServiceRepository.GetQuery().FirstOrDefault(x => x.Name == UknownWebServiceName);
        // 
        //             var activityTemplate = _activityTemplate.GetQuery()
        //                 .Where(x => x.ActivityTemplateState == ActivityTemplateState.Active)
        //                 .Where(x => id == 0 || category == x.Category)
        //                 .Where(x => x.Tags == null || (!x.Tags.Contains(Tags.Internal) || _fr8Account.IsCurrentUserInAdminRole()));
        // 
        //             webServiceList = activityTemplate
        //             .GroupBy(x => x.WebService, x => x, (key, group) => new
        //             {
        //                 WebService = key,
        //                 SortOrder = key == null ? 1 : 0,
        //                 Actions = group
        //             }).OrderBy(x => x.SortOrder)
        //             .Select(x => new WebServiceActivitySetDTO
        //             {
        //                 WebServiceIconPath = x.WebService != null ? x.WebService.IconPath : (unknwonService != null ? unknwonService.IconPath : null),
        //                 WebServiceName = x.WebService != null ? x.WebService.Name : string.Empty,
        //                 Activities = x.Actions
        //                  .GroupBy(y => y.Name)
        //                  .Select(y => y.OrderByDescending(z => int.Parse(z.Version)).First())
        //                  .Select(p => new ActivityTemplateDTO
        //                  {
        //                      Id = p.Id,
        //                      Name = p.Name,
        //                      Category = p.Category,
        //                      Label = p.Label,
        //                      MinPaneWidth = p.MinPaneWidth,
        //                      Version = p.Version,
        //                      Type = p.Type,
        //                      Tags = p.Tags,
        //                      WebService = Mapper.Map<WebServiceDTO>(p.WebService),
        //                      Terminal = Mapper.Map<TerminalDTO>(p.Terminal)
        //                  }).ToList()
        //             }).ToList();
        //         }
        // 
        //         return Ok(webServiceList);
        //     }
        // 
        //     // If there is no category
        //     using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //     {
        //         var models = uow.WebServiceRepository
        //             .GetAll()
        //             .Select(Mapper.Map<WebServiceDTO>)
        //             .ToList();
        //         return Ok(models);
        //     }
        // }

        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, "Collection of web services including activity templates", typeof(List<WebServiceActivitySetDTO>))]
        public IHttpActionResult Get(Guid? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (id.HasValue)
                {
                    if (!ActivityCategories.ActivityCategoryIds.Contains(id.Value))
                    {
                        throw new ApplicationException("Invalid predefined activity category");
                    }

                    var idValue = id.Value;
                    var activityTemplates = uow
                        .ActivityTemplateRepository
                        .GetQuery()
                        .Include("Categories.ActivityCategory")
                        .Where(x => x.Categories.Any(y => y.ActivityCategoryId == idValue))
                        .ToList();

                    var categorySet = new HashSet<ActivityCategoryDO>();
                    foreach (var category in activityTemplates.SelectMany(x => x.Categories).Select(x => x.ActivityCategory))
                    {
                        if (ActivityCategories.ActivityCategoryIds.Contains(category.Id))
                        {
                            continue;
                        }

                        categorySet.Add(category);
                    }

                    var result = categorySet
                        .OrderBy(x => x.Name)
                        .Select(x => new WebServiceActivitySetDTO()
                        {
                            WebServiceName = x.Name,
                            WebServiceIconPath = x.IconPath,
                            Activities = activityTemplates
                                .Where(y => y.Categories.Any(z => z.ActivityCategoryId == x.Id))
                                .GroupBy(y => new { y.Name })
                                .Select(y => Mapper.Map<ActivityTemplateDTO>(y.OrderByDescending(z => int.Parse(z.Version)).First()))
                                .ToList()
                        })
                        .ToList();

                    return Ok(result);
                }
                else
                {
                    var models = uow.ActivityCategoryRepository
                        .GetAll()
                        .Select(Mapper.Map<ActivityCategoryDTO>)
                        .ToList();

                    return Ok(models);
                }
            }
        }

        /// <summary>
        /// Creates web service with specified data
        /// </summary>
        /// <param name="webService">Web service data to create web service from</param>
        /// <response code="200">Web service was successfully saved</response>
        [HttpPost]
        [ResponseType(typeof(ActivityCategoryDTO))]
        public IHttpActionResult Post(ActivityCategoryDTO category)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var existingCategory = uow.ActivityCategoryRepository
                    .GetQuery()
                    .Where(x => x.Name == category.Name)
                    .FirstOrDefault();

                ActivityCategoryDTO model;

                if (existingCategory != null)
                {
                    existingCategory.IconPath = category.IconPath;
                    existingCategory.Type = category.Type;
                    model = Mapper.Map<ActivityCategoryDTO>(existingCategory);
                }
                else
                {
                    var entity = Mapper.Map<ActivityCategoryDO>(category);
                    entity.Id = Guid.NewGuid();

                    uow.ActivityCategoryRepository.Add(entity);
                    model = Mapper.Map<ActivityCategoryDTO>(entity);
                }

                uow.SaveChanges();

                return Ok(model);
            }
        }
    }
}