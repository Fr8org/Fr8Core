using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    //This class simple removes name of model from some parameter names
    //In case your controller's method has complex object as a parameter and it is marked by [FromUri] attribute
    //Then Swashbuckle by default will show these as 'parameterName.propertyName'
    public class RemoveParameterModelNameOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
            {
                return;
            }
            foreach (var parameter in operation.parameters)
            {
                parameter.name = parameter.name.Split('.').Last();
            }
        }
    }
}