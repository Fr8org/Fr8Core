using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using StructureMap;
using StructureMap.Pipeline;
using Swashbuckle.Swagger;

namespace HubWeb.Documentation.Swagger
{
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
            _defaultFactoriesByTypeName = defaultValueFactories.ToDictionary(x => x.Interface.GetGenericArguments()[0].Name, x => x.Interface);
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