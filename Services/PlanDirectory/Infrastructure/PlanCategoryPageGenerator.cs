using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Data.Entities;
using PlanDirectory.CategoryPages;

namespace PlanDirectory.Infrastructure
{
    public class PlanCategoryPageGenerator : IPageGenerator
    {
        private const string TagsSeparator = "-";
        private const string PageExtension = ".html";

        public void Generate(IEnumerable<string> tags)
        {
            var path = @"D:\\Dev\\fr8company\\Services\\PlanDirectory\\CategoryPages\\";
            var fileName = GeneratePageNameFromTags(tags);
            var template = new PlanCategoryTemplate();
            template.Session = new Dictionary<string, object>
            {
                ["Name"] = fileName,
                ["Tags"] = tags.OrderBy(x => x).ToList()
            };
            template.Initialize(); // Must call this to transfer values.

            string pageContent = template.TransformText();
            File.WriteAllText(path + fileName + PageExtension, pageContent);
        }

        /// <summary>
        /// Generates pageName from tags
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private string GeneratePageNameFromTags(IEnumerable<string> tags)
        {
            return string.Join(
                TagsSeparator,
                tags.Select(x => x.ToLower()).OrderBy(x => x));
        }
    }
}