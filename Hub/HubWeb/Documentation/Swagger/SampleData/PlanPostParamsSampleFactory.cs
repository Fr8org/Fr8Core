using Fr8.Infrastructure.Documentation.Swagger;
using HubWeb.ViewModels.RequestParameters;

namespace HubWeb.Documentation.Swagger
{
    public class PlanPostParamsSampleFactory : ISwaggerSampleFactory<PlansPostParams>
    {
        public PlansPostParams GetSampleData()
        {
            return new PlansPostParams
            {
                solutionName = "Track_DocuSign_Recipients",
                updateRegistrations = false
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}