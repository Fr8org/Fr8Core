using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using Data.Entities;
using Data.Repositories;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using PlanDirectory.CategoryPages;
using StructureMap;

namespace PlanDirectory.Infrastructure
{
    public class PageGenerator : IPageGenerator
    {
        private const string TagsSeparator = "-";
        private const string PageExtension = ".html";
        private const string CategoryPagesDir = "categorypages";

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
            var serverPath = HostingEnvironment.MapPath("~");
            if (serverPath == null)
            {
                var uriPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                serverPath = new Uri(uriPath).LocalPath;
            }
            var path = Path.Combine(serverPath, CategoryPagesDir);

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
                    var pdFileName = GeneratePageNameFromTags(pageDefinitionDO.Tags);
                    pageDefinitionDO.PageName = pdFileName;
                    pageDefinitionDO.Url = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") 
                        + "/" + CategoryPagesDir 
                        + "/" + pdFileName);
                    _pageDefinition.CreateOrUpdate(pageDefinitionDO);
                }
                var fileName = GeneratePageNameFromTags(tag.TagsWithIcons.Select(x => x.Key));
                var relatedPageDefinitions =
                    _pageDefinition.GetAll().Where(x => x.PlanTemplatesIds.Contains(planTemplate.ParentPlanId));


                var curPageDefinition = relatedPageDefinitions.FirstOrDefault(x => x.PageName == fileName);
                var curRelatedPlans = new List<PublishPlanTemplateDTO>();
                foreach (var planTemplateId in curPageDefinition.PlanTemplatesIds)
                {
                    curRelatedPlans.Add(_planTemplate.Get(fr8AccountId, Guid.Parse(planTemplateId)).Result);
                }

                var relatedPlans = new List<Tuple<string, string, string>>();
                foreach (var publishPlanTemplateDTO in curRelatedPlans)
                {
                    relatedPlans.Add(new
                        Tuple<string, string, string>(
                        publishPlanTemplateDTO.Name,
                        publishPlanTemplateDTO.Description ?? publishPlanTemplateDTO.Name,
                        CloudConfigurationManager.GetSetting("HubApiBaseUrl").Replace("/api/v1/", "")
                        + "/dashboard/plans/" + publishPlanTemplateDTO.ParentPlanId + "/builder?viewMode=plan"));
                }
                var template = new PlanCategoryTemplate();
                template.Session = new Dictionary<string, object>
                {
                    ["Name"] = fileName,
                    ["Tags"] = tag.TagsWithIcons,
                    ["RelatedPlans"] = relatedPlans
                };
                // Must call this to transfer values.
                template.Initialize();

                string pageContent = template.TransformText();
                File.WriteAllText(path + "\\" + fileName, pageContent);
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
                tagsTitles.Select(x => x.ToLower()).OrderBy(x => x)) + PageExtension;
        }
    }
}