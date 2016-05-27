using System;
using TerminalBase.Interfaces;
using TerminalBase.Models;

namespace TerminalBase.Services
{
    public class DefaultActivityFactory : IActivityFactory
    {
        private readonly Type _type;

        public DefaultActivityFactory(Type type)
        {
            _type = type;
        }

        public IActivity Create()
        {
            return (IActivity)Activator.CreateInstance(_type);
        }
    }
}
