using System;
using System.Linq;
using Fr8.TerminalBase.Interfaces;
using StructureMap;

namespace Fr8.TerminalBase.Services
{
    public class DefaultActivityFactory : IActivityFactory
    {
        private readonly Type _type;
        private readonly IContainer _container;

        public DefaultActivityFactory(Type type, IContainer container)
        {
            _type = type;
            _container = container;
        }

        public IActivity Create()
        {
            var firstConstructor = _type.GetConstructors().OrderBy(x => x.GetParameters().Length).FirstOrDefault();

            if (firstConstructor == null)
            {
                throw new Exception("Unable to find constructor for activity type: " + _type);
            }

            var parameters = firstConstructor.GetParameters();
            var paramArguments = new object[parameters.Length];

            for (int index = 0; index < parameters.Length; index++)
            {
                var parameterInfo = parameters[index];
                paramArguments[index] = _container.GetInstance(parameterInfo.ParameterType);
            }

            var instance = firstConstructor.Invoke(paramArguments.ToArray()) as IActivity;

            if (instance == null)
            {
                throw new Exception("Unable to create instance of type: " + _type);
            }

            return instance;
        }
    }
}