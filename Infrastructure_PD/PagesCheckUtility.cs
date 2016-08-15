using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanDirectory;
using Hub.Interfaces;

namespace HubWeb.Infrastructure_PD
{
    public class PagesCheckUtility:IPagesCheckUtility
    {
        private readonly IPlanTemplateDetailsGenerator _planTemplateDetailsGenerator;
        private readonly ISearchProvider _searchProvider;

        public PagesCheckUtility(IPlanTemplateDetailsGenerator planTemplateDetailsGenerator,
                                        ISearchProvider searchProvider)
        {
            _planTemplateDetailsGenerator = planTemplateDetailsGenerator;
            _searchProvider = searchProvider;
        }

        public async Task<long> CheckPlanTempletesPages()
        {
            var result = 0;
            
            var searchResult = await _searchProvider.Search(new SearchRequestDTO());

            // all this will be refactored in testable manner

            var serverPath = HostingEnvironment.MapPath("~");
            if (serverPath == null)
            {
                var uriPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                serverPath = new Uri(uriPath).LocalPath + "\\details";
            }

            var directoryFiles = System.IO.Directory.GetFiles(serverPath);

            foreach (var planTemplate in searchResult.PlanTemplates)
            {
                var filename = planTemplate.Name + "-" + planTemplate.ParentPlanId + ".html";

                if (!System.IO.File.Exists(filename))
                {
                    // 
                    // do we need map SearchItemDTO to PublishPlanTemplateDTO?
                    var item = new PublishPlanTemplateDTO()
                    {
                        Name = planTemplate.Name,
                        Description = planTemplate.Description,
                        ParentPlanId = planTemplate.ParentPlanId
                    };

                    await _planTemplateDetailsGenerator.Generate(item);
                }
            }

            return searchResult.TotalCount;
        }
    }
}