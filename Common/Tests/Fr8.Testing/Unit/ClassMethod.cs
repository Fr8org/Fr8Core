using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fr8.Testing.Unit
{
    public static class ClassMethod
    {
        public static object Invoke(Type calledType, string methodName, Object[] parameters)
        {
            try
            {
                MethodInfo curMethodInfo = calledType.GetMethod(methodName,
                    BindingFlags.Default |
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.InvokeMethod |
                    BindingFlags.CreateInstance |
                    BindingFlags.GetField |
                    BindingFlags.SetField |
                    BindingFlags.GetProperty |
                    BindingFlags.SetProperty |
                    BindingFlags.PutDispProperty |
                    BindingFlags.PutRefDispProperty |
                    BindingFlags.ExactBinding |
                    BindingFlags.SuppressChangeType |
                    BindingFlags.OptionalParamBinding |
                    BindingFlags.IgnoreReturn
                );

                ParameterInfo[] curMethodParameters = curMethodInfo.GetParameters();
                object curObject = Activator.CreateInstance(calledType);
                var response = (object)curMethodInfo.Invoke(curObject, curMethodParameters.Length == 0 ? null : parameters);
                return response;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }

        }
    }
}
