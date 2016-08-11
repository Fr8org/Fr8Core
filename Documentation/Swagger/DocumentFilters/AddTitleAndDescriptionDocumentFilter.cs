using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace HubWeb.Documentation.Swagger.DocumentFilters
{
    public class AddTitleAndDescriptionDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.info.title = "Fr8 Hub Endpoints";
            swaggerDoc.info.description =
"This is the place where you can find all you need to know about REST API for [Fr8 services](http://fr8.co) including endpoints description, parameters used, sample values for any"
+ " complex type used both as input parameter and return values, response codes etc. Learn more on how to try endpoints that require authentication [here](https://github.com/Fr8org/Fr8Core/tree/master/Docs/ForDevelopers/Specifications/TryingOutHubApi.md)";
        }
    }
}