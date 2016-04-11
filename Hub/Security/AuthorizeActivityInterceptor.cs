using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Castle.Core.Interceptor;
using Data.Infrastructure.Security;
using Data.Infrastructure.StructureMap;
using StructureMap;

namespace Hub.Security
{
    /// <summary>
    /// AOP Interceptor used to invoke some code on all methods/properties that has been decorated with AuthorizeActivitiyAttribute.
    /// This method will extract needed data from attribute and method/property that is invocation target and will check if user has privileges to do some activity on invocation target.
    /// Config: Lifecycle of interceptor is configured inside StructureMap bootstrapper. For new classes that will have security checks add the dynamic proxy generator as decorator:
    /// Example: For<IActivity>().Use<Activity>().DecorateWith(z => dynamicProxy.CreateInterfaceProxyWithTarget(z, new AuthorizeActivityInterceptor()));
    /// </summary>
    public class AuthorizeActivityInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            AuthorizeActivity(invocation);
        }

        /// <summary>
        /// On Method call, this functionality is invoked. Get needed data about privilege and secured object target that need to be authorized
        /// </summary>
        /// <param name="invocation"></param>
        private void AuthorizeActivity(IInvocation invocation)
        {
            if (!IsMethodMarkedForAuthorization(invocation.MethodInvocationTarget))
            {
                invocation.Proceed();
                return;
            }

            var authorizeAttribute = (invocation.MethodInvocationTarget.GetCustomAttributes(typeof(AuthorizeActivityAttribute), true).First()
                as AuthorizeActivityAttribute ?? new AuthorizeActivityAttribute());

            switch (invocation.Method.MemberType)
            {
                case MemberTypes.Property:
                    AuthorizePropertyInvocation(invocation, authorizeAttribute);
                    break;
                case MemberTypes.Method:
                    AuthorizeMethodInvocation(invocation, authorizeAttribute);
                    break;
            }
        }

        /// <summary>
        /// Authorization logic for Methods that has been decorated with AuthorizeActivityAttribute 
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="authorizeAttribute"></param>
        private void AuthorizeMethodInvocation(IInvocation invocation, AuthorizeActivityAttribute authorizeAttribute)
        {
            foreach (var parameter in MapParameters(invocation.Arguments, invocation.Method.GetParameters(), authorizeAttribute.ObjectType))
            {
                string objectId;
                if (parameter is Guid || parameter is string)
                {
                    objectId = (string)parameter;
                }
                else
                {
                    //todo: in case of requirement for objects not inherited from BaseObject, create a new property inside AuthorizeActivityAttribute that will set object inner propertyName in case of this "Id"  
                    var property = parameter.GetType().GetProperty("Id");
                    objectId = property.GetValue(parameter).ToString();
                }
                
                ISecurityServices securityServices = ObjectFactory.GetInstance<ISecurityServices>();
                if (securityServices.AuthorizeActivity(authorizeAttribute.Privilege, objectId))
                {
                    invocation.Proceed();
                }
                else
                {
                    throw new HttpException(401, "You are not authorized to perform this activity!");
                }
            }
        }

        /// <summary>
        /// Authorization logic for Properties that has been decorated with AuthorizeActivityAttribute 
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="authorizeAttribute"></param>
        private void AuthorizePropertyInvocation(IInvocation invocation, AuthorizeActivityAttribute authorizeAttribute)
        {
            var property = invocation.Method.GetType().GetProperty("Id");
            var objectId = property.GetValue(property).ToString();
            
            ISecurityServices securityServices = ObjectFactory.GetInstance<ISecurityServices>();
            if (securityServices.AuthorizeActivity(authorizeAttribute.Privilege, objectId))
            {
                invocation.Proceed();
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        /// <summary>
        /// Allow only methods/properties that has AuthorizeActivityAttribute set on them
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private bool IsMethodMarkedForAuthorization(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(AuthorizeActivityAttribute), true).Any();
        }

        /// <summary>
        /// Map parameters from Method with invocation arguments and filter them by some objectType
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="getParameters"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private IEnumerable<object> MapParameters(object[] arguments, ParameterInfo[] getParameters, Type objectType)
        {
            var objectsForAuthorization = new List<object>();
            for (var i = 0; i < arguments.Length; i++)
            {
                var argumentType = (arguments[i]).GetType();
                if (argumentType == objectType && argumentType == getParameters[i].ParameterType)
                {
                    objectsForAuthorization.Add(arguments[i]);
                }
            }
            return objectsForAuthorization;
        }
    }
}
