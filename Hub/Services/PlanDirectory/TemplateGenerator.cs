using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hub.Interfaces;

namespace Hub.Services.PlanDirectory
{
    public class TemplateGenerator : ITemplateGenerator
    {
        public TemplateGenerator(Uri baseUrl, string outputFolder)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                throw new ArgumentException("Value can't be empty", nameof(outputFolder));
            }
            BaseUrl = baseUrl;
            OutputFolder = outputFolder;
        }

        public TemplateGenerator(string baseUrl, string outputFolder) : this(new Uri(baseUrl), outputFolder)
        {
        }

        public Uri BaseUrl { get; set; }

        public string OutputFolder { get; set; }
        
        public async Task Generate(dynamic template, string fileName, IDictionary<string, object> parameters = null)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Value can't be empty", nameof(fileName));
            }
            template.Session = parameters ?? new Dictionary<string, object>(0);
            template.Initialize();
            var pageContent = (string)template.TransformText();
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }
            using (var writer = new StreamWriter(Path.Combine(OutputFolder, fileName)))
            {
                await writer.WriteAsync(pageContent);
            }
        }
    }
}