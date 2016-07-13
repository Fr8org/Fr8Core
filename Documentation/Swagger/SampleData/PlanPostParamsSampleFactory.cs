using HubWeb.ViewModels.RequestParameters;

namespace HubWeb.Documentation.Swagger
{
    public class PlanPostParamsSampleFactory : ISwaggerSampleFactory<PlansPostParams>
    {
        public PlansPostParams GetSampleData()
        {
            return new PlansPostParams
            {
                solution_name = "Track_DocuSign_Recipients",
                update_registrations = false
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}