using Data.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
       // public override string BaseUrl { get; }

       // public override IEnumerable<string> AvailableCommands { get; }

        public string InvokeMethod(string typeName, string methodName, ActionRegistrationDO curActionRegistrationDO)
        {
            try
            {
                // Get the Type for the class
                Type calledType = Type.GetType(typeName);
                MethodInfo curMethodInfo = calledType.GetMethod(methodName);
                object curObject = Activator.CreateInstance(calledType);
                return (string)curMethodInfo.Invoke(curObject, new Object[] { curActionRegistrationDO });
            }
            catch (Exception)
            {
                
                throw;
            }
           
        }

        public string BaseUrl
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> AvailableCommands
        {
            get { throw new NotImplementedException(); }
        }
    }
}