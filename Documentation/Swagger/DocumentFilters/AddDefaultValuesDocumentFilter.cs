using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using StructureMap;
using StructureMap.Pipeline;
using Swashbuckle.Swagger;

namespace HubWeb.Documentation.Swagger
{
    //This class produces sample data for complex objects to be displayed in Swagger UI
    //In order to provide sample value for your type just add a class that implements ISwaggerSampleFactory<YourType> 
    //You can rely on constructor injection of other implentations in order to reuse default values for those type
    //But make sure first that such class doesn't already exist. See examples in Documentation/Swagger/SampleData folder
    public class AddDefaultValuesDocumentFilter : IDocumentFilter
    {
        private readonly IContainer _container;

        private readonly Dictionary<string, Type> _defaultFactoriesByTypeName;
        public AddDefaultValuesDocumentFilter()
        {
            _container = new Container();
            var thisAssembly = GetType().Assembly;
            var defaultValueFactories = thisAssembly.GetTypes()
                .Where(x => x.IsClass)
                .Select(x => new {Type = x, Interface = x.GetInterface("ISwaggerSampleFactory`1")})
                .Where(x => x.Interface != null)
                .ToArray();
            _container.Configure(x =>
            {
                foreach (var defaultValueFactory in defaultValueFactories)
                {
                    x.For(defaultValueFactory.Interface, new SingletonLifecycle()).Use(defaultValueFactory.Type);
                }
            });
            _defaultFactoriesByTypeName = defaultValueFactories.ToDictionary(x => GetSwaggerTypeName(x.Interface), x => x.Interface);
        }

        private string GetSwaggerTypeName(Type @interface)
        {
            var genericType = @interface.GetGenericArguments()[0];
            if (genericType.IsGenericType)
            {
                var result = $"{genericType.Name.Remove(genericType.Name.IndexOf('`'))}[{string.Join(",", genericType.GenericTypeArguments.Select(x => x.Name))}]";
                return result;
            }
            return genericType.Name;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var schema in schemaRegistry.Definitions)
            {
                Type defaultFactoryType;
                if (_defaultFactoriesByTypeName.TryGetValue(schema.Key, out defaultFactoryType))
                {
                    schema.Value.example = (_container.GetInstance(defaultFactoryType) as ISwaggerSampleFactory).GetSampleData();
                }
            }
        }
    }
}