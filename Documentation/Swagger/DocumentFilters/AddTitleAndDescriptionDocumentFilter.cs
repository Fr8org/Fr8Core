using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace HubWeb.Documentation.Swagger.DocumentFilters
{
    public class AddTitleAndDescriptionDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.info.title = "Fr8 Hub API";
            swaggerDoc.info.description =
                "These API's are supported by Fr8 Hubs. Most calls require authentication. Learn about that [here](https://github.com/Fr8org/Fr8Core/tree/master/Docs/ForDevelopers/Specifications/TryingOutHubApi.md). "
                + "Fr8 Hubs communicate with the client and with Fr8 Terminals. The Fr8 Terminal API is [here](http://dev-terminals.fr8.co:25923/swagger/ui/index).<br />"
                + "Learn More<br />[Fr8 Developer Home](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)<br />[Fr8 Hub Home](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Fr8Hubs.md)";
        }
    }
}