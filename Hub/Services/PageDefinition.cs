using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Validation.Providers;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class PageDefinition : IPageDefinition
    {
        private const string TagsSeparator = "-";
        private const string PageExtension = ".html";

        public IEnumerable<PageDefinitionDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.PageDefinitionRepository.GetAll();
            }
        }

        public PageDefinitionDO Get(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.PageDefinitionRepository.GetByKey(id);
            }
        }

        public void CreateOrUpdate(PageDefinitionDO pageDefinitionDO)
        {
            pageDefinitionDO.PageName = GeneratePageNameFromTags(pageDefinitionDO);
            if (pageDefinitionDO.Id > 0)
            {

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var pageDefinitionToUpdate = uow.PageDefinitionRepository.GetByKey(pageDefinitionDO.Id);
                    pageDefinitionToUpdate.Title = pageDefinitionDO.Title;
                    pageDefinitionToUpdate.Description = pageDefinitionDO.Description;
                    pageDefinitionToUpdate.PageName = pageDefinitionDO.PageName;
                    pageDefinitionToUpdate.Tags = pageDefinitionDO.Tags;
                    pageDefinitionToUpdate.Type = pageDefinitionDO.Type;
                    pageDefinitionToUpdate.Url = pageDefinitionDO.Url;
                    pageDefinitionToUpdate.LastUpdated = DateTimeOffset.Now;
                    uow.SaveChanges();
                }
            }
            else
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    uow.PageDefinitionRepository.Add(pageDefinitionDO);
                    uow.SaveChanges();
                }
            }
        }

        public void Delete(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var pageDefinitionToRemove = uow.PageDefinitionRepository.GetByKey(id);
                uow.PageDefinitionRepository.Remove(pageDefinitionToRemove);
            }
        }

        /// <summary>
        /// Generates pageName from tags
        /// </summary>
        /// <param name="pageDefinitionDO"></param>
        /// <returns></returns>
        private static string GeneratePageNameFromTags(PageDefinitionDO pageDefinitionDO)
        {
            return string.Join(
                TagsSeparator,
                pageDefinitionDO.Tags.Select(x => x.ToLower()).OrderBy(x => x)) + PageExtension;
        }
    }
}
