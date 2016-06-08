using System.Linq;
using AutoMapper;

namespace Fr8.Infrastructure.Utilities
{
    public static class MappingEngineExtensions
    {
        public static T Map<T>(this IMappingEngine mappingEngine, params object[] sources) where T : class
        {
            if (!sources.Any())
            {
                return default(T);
            }

            var initialSource = sources[0];

            var mappingResult = Map<T>(mappingEngine, initialSource);

            // Now map the remaining source objects
            if (sources.Count() > 1)
            {
                Map(mappingEngine, mappingResult, sources.Skip(1).ToArray());
            }

            return mappingResult;
        }

        private static void Map(this IMappingEngine mappingEngine, object destination, params object[] sources)
        {
            if (!sources.Any())
            {
                return;
            }

            var destinationType = destination.GetType();

            foreach (var source in sources)
            {
                var sourceType = source.GetType();
                mappingEngine.Map(source, destination, sourceType, destinationType);
            }
        }

        private static T Map<T>(this IMappingEngine mappingEngine, object source) where T : class
        {
            var destinationType = typeof(T);
            var sourceType = source.GetType();

            var mappingResult = mappingEngine.Map<T>(source, sourceType, destinationType);

            return mappingResult;
        }
    }
}
