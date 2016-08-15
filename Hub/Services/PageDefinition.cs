using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Validation.Providers;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Hub.Interfaces;
using StructureMap;
using Newtonsoft.Json;

namespace Hub.Services
{
    public class PageDefinition : IPageDefinition
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        public PageDefinition(IUnitOfWorkFactory uowFactory)
        {
            _uowFactory = uowFactory;
        }

        public IEnumerable<PageDefinitionDO> GetAll()
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.PageDefinitionRepository.GetAll();
            }
        }

        public PageDefinitionDO Get(int id)
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.PageDefinitionRepository.GetByKey(id);
            }
        }

        public PageDefinitionDO Get(IEnumerable<string> tags)
        {
            using (var uow = _uowFactory.Create())
            {
                var tagsString = JsonConvert.SerializeObject(tags.OrderBy(x => x));
                return uow.PageDefinitionRepository
                    .GetQuery()
                    .Where(x => x.TagsString == tagsString)
                    .FirstOrDefault();
            }
        }

        public IList<PageDefinitionDO> Get(Expression<Func<PageDefinitionDO, bool>> filter)
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.PageDefinitionRepository.GetQuery().Where(filter).ToArray();
            }
        }

        public void CreateOrUpdate(PageDefinitionDO pageDefinitionDO)
        {
            if (pageDefinitionDO.Id > 0)
            {
                using (var uow = _uowFactory.Create())
                {
                    var pageDefinitionToUpdate = uow.PageDefinitionRepository.GetByKey(pageDefinitionDO.Id);
                    pageDefinitionToUpdate.Title = pageDefinitionDO.Title;
                    pageDefinitionToUpdate.Description = pageDefinitionDO.Description;
                    pageDefinitionToUpdate.PageName = pageDefinitionDO.PageName;
                    pageDefinitionToUpdate.Tags = pageDefinitionDO.Tags;
                    pageDefinitionToUpdate.Type = pageDefinitionDO.Type;
                    pageDefinitionToUpdate.Url = pageDefinitionDO.Url;
                    pageDefinitionToUpdate.LastUpdated = DateTimeOffset.Now;
                    pageDefinitionToUpdate.PlanTemplatesIds.AddRange(pageDefinitionDO.PlanTemplatesIds);
                    uow.SaveChanges();
                }
            }
            else
            {
                using (var uow = _uowFactory.Create())
                {
                    var existedPd = uow.PageDefinitionRepository.FindOne(x => x.PageName == pageDefinitionDO.PageName && x.Type == pageDefinitionDO.Type);
                    if (existedPd == null)
                    {
                        uow.PageDefinitionRepository.Add(pageDefinitionDO);
                        uow.SaveChanges();
                    }
                    else
                    {
                        existedPd.Title = pageDefinitionDO.Title;
                        existedPd.Description = pageDefinitionDO.Description;
                        existedPd.PageName = pageDefinitionDO.PageName;
                        existedPd.Tags = pageDefinitionDO.Tags;
                        existedPd.Type = pageDefinitionDO.Type;
                        existedPd.Url = pageDefinitionDO.Url;
                        existedPd.LastUpdated = DateTimeOffset.Now;
                        existedPd.PlanTemplatesIds.AddRange(pageDefinitionDO.PlanTemplatesIds);
                        uow.SaveChanges();
                    }
                }
            }
        }

        public void Delete(int id)
        {
            using (var uow = _uowFactory.Create())
            {
                var pageDefinitionToRemove = uow.PageDefinitionRepository.GetByKey(id);
                uow.PageDefinitionRepository.Remove(pageDefinitionToRemove);
            }
        }
    }
}
