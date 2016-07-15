using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace HubWeb.Documentation.Swagger.DocumentFilters
{
    public class AddTitleAndDescriptionDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.info.title = "Fr8 Swagger";
            swaggerDoc.info.description = "This is the place where you can find all you need to know about REST API for [Fr8 services](http://fr8.co). In order to test the authorization filters you may"
                                          + " first perform cookie-based authentication using <strong>Authentication/Login</strong> method";
        }
    }
}