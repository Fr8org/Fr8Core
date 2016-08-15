using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

        public async Task<int> CheckPlanTempletesPages()
        {
            var result = 0;

            var emptySearchRequest = new SearchRequestDTO()
            {
            };

            var searchResult = await _searchProvider.Search(emptySearchRequest);

            return result;
        }
    }
}