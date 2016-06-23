using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Data.Entities;
using Data.Repositories;
using PlanDirectory.CategoryPages;

namespace PlanDirectory.Infrastructure
{
    public class PlanCategoryPageGenerator : IPageGenerator
    {
        private const string TagsSeparator = "-";
        private const string PageExtension = ".html";

        private readonly IPageDefinitionRepository _pageDefinitionRepository;

        public PlanCategoryPageGenerator(IPageDefinitionRepository pageDefinitionRepository)
        {
            _pageDefinitionRepository = pageDefinitionRepository;
        }

        public void Generate(IEnumerable<WebServiceTemplateTag> tags)
        {
            var path = @"D:\\Dev\\fr8company\\Services\\PlanDirectory\\CategoryPages\\";
            var pageDefinitions = _pageDefinitionRepository.GetAll();
            foreach (var webServiceTemplateTag in tags)
            {
                
            }

            foreach (var webServiceTemplateTag in tags)
            {
                var fileName = GeneratePageNameFromTags(webServiceTemplateTag.TagsWithIcons.Select(x => x.Key));
                var template = new PlanCategoryTemplate();
                template.Session = new Dictionary<string, object>
                {
                    ["Name"] = fileName,
                    ["Tags"] = webServiceTemplateTag.TagsWithIcons
                };
                // Must call this to transfer values.
                template.Initialize();

                string pageContent = template.TransformText();
                File.WriteAllText(path + fileName + PageExtension, pageContent);
            }
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