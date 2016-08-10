using Fr8.Infrastructure.Documentation.Swagger;

namespace HubWeb.Documentation.Swagger
{
    //This class will allow to apply sample data for types specific for Hub while still having ability to refer generic sample data
    public class AddHubDefaultValuesDocumentFilter : AddDefaultValuesDocumentFilter
    {
        public AddHubDefaultValuesDocumentFilter() : base()
        {
            AddDefaultFactoriesFromTypeAssembly(typeof(AddHubDefaultValuesDocumentFilter));
        }
    }
}