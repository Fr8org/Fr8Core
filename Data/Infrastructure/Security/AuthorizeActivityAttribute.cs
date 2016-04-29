using System;
using Data.States;

namespace Data.Infrastructure.Security
{
    /// <summary>
    /// Attribute used for Security purposes, to determine if logged in user can do CRUD operations. 
    /// This attribute will be intercepted by additional logic that will invoke security checks.
    /// In case set to methods, must be set parameter ObjectType. That way interceptor will know what method parameters 
    ///    to extract from method parameters and and to find our object identifier for check.
    /// In case attribute is used on properties, We don't need to set ObjectType because interceptor will look inside current object to find key property
    ///    reflect current property to look for Key item of type Guid 
    /// Example :  
    ///          [AuthorizeActivity(Permission = Permission.ReadObject, ObjectType = typeOf(PlanDO)]
    ///          public PlanDO Copy(IUnitOfWork uow, PlanDO curPlanDO, string name) {
    /// so we can know to look for second parameter and to find inside of curPlanDO object his Id value
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class AuthorizeActivityAttribute : Attribute
    {
        public AuthorizeActivityAttribute()
        {
            ParamType = typeof(Guid);
            Permission = PermissionType.ReadObject;
        }

        /// <summary>
        /// Permission name that must be checked for authorization 
        /// </summary>
        public PermissionType Permission { get; set; }

        /// <summary>
        /// Type of the argument/parameter where dataObjectId is located. 
        /// Default values is Guid, and could be used with BaseObjects that contains key as Id 
        /// </summary>
        public Type ParamType { get; set; }

        /// <summary>
        /// Global Object type for whom we are checking granted permissions
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Attribute is set on a property
        /// </summary>
        public bool IsProperty { get; set; }
    }
}
