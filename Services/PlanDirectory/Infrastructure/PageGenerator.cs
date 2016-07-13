using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using Data.Entities;
using Data.Repositories;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Interfaces;
using PlanDirectory.CategoryPages;
using StructureMap;

namespace PlanDirectory.Infrastructure
{
    public class PageGenerator : IPageGenerator
    {
        private const string TagsSeparator = "-";
        private const string PageExtension = ".html";

        private readonly IPageDefinition _pageDefinition;
        private readonly IPlanTemplate _planTemplate;

        public PageGenerator(IPageDefinition pageDefinition)
        {
            _pageDefinition = pageDefinition;
            _planTemplate = ObjectFactory.GetInstance<IPlanTemplate>();
        }

        public Task Generate(
            TemplateTagStorage storage,
            PlanTemplateCM planTemplate,
            IList<PageDefinitionDO> pageDefinitions,
            string fr8AccountId)
        {
            var path = Path.Combine(HostingEnvironment.MapPath("~"), "CategoryPages");

            foreach (var tag in storage.WebServiceTemplateTags)
            {
                foreach (var pageDefinitionDO in pageDefinitions)
                {

                    if (pageDefinitionDO.PlanTemplatesIds == null)
                    {
                        pageDefinitionDO.PlanTemplatesIds = new List<string> { planTemplate.ParentPlanId };
                    }
                    else
                    {
                        if (!pageDefinitionDO.PlanTemplatesIds.Contains(planTemplate.ParentPlanId))
                            pageDefinitionDO.PlanTemplatesIds.Add(planTemplate.ParentPlanId);
                    }
                    _pageDefinition.CreateOrUpdate(pageDefinitionDO);
                }

                var relatedPageDefinitions =
                    _pageDefinition.GetAll().Where(x => x.PlanTemplatesIds.Contains(planTemplate.ParentPlanId));

                var fileName = GeneratePageNameFromTags(tag.TagsWithIcons.Select(x => x.Key));
                var curPageDefinition = relatedPageDefinitions.FirstOrDefault(x => x.PageName == fileName + PageExtension);
                var curRelatedPlans = new List<PublishPlanTemplateDTO>();
                foreach (var planTemplateId in curPageDefinition.PlanTemplatesIds)
                {
                    curRelatedPlans.Add(_planTemplate.Get(fr8AccountId, Guid.Parse(planTemplateId)).Result);
                }
                var template = new PlanCategoryTemplate();
                template.Session = new Dictionary<string, object>
                {
                    ["Name"] = fileName,
                    ["Tags"] = tag.TagsWithIcons,
                    ["RelatedPlans"] = curRelatedPlans.ToDictionary(x => x.Name, x => x.Description ?? x.Name)
                };
                // Must call this to transfer values.
                template.Initialize();

                string pageContent = template.TransformText();
                File.WriteAllText(path + "\\" + fileName + PageExtension, pageContent);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Generates pageName from tagsTitles
        /// </summary>
        /// <param name="tagsTitles"></param>
        /// <returns></returns>
        private static string GeneratePageNameFromTags(IEnumerable<string> tagsTitles)
        {
            return string.Join(
                TagsSeparator,
                tagsTitles.Select(x => x.ToLower()).OrderBy(x => x));
        }
    }
}