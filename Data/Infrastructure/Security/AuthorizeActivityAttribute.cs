using System;
using Data.States;

namespace Data.Infrastructure.Security
{
    /// <summary>
    /// Attribute used for Security purposes, to determine if logged in user can do CRUD operations. 
    /// This attribute will be intercepted by additional logic that will invoke security checks.
    /// In case attribute is used on methods must be set the parameter ObjectIdArgumentIndex, 
    ///    which tells us the index of parameter where we need to look for data object identifier from arguments array.
    /// In case attribute is used on properties, set IsProperty flag to true, which will tells us on interception of that property will reflect current property to look for Key item if type Guid 
    /// Example :  
    ///          [AuthorizeActivity(Privilege = Privilege.ReadObject, ObjectIdArgumentIndex = 1)]
    ///          public PlanDO Copy(IUnitOfWork uow, PlanDO curPlanDO, string name) {
    /// so we can know to look for second parameter and to find inside of object his Id value
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class AuthorizeActivityAttribute : Attribute
    {
        public AuthorizeActivityAttribute()
        {
            ObjectIdArgumentIndex = 0;
            Privilege = Privilege.ReadObject;
            IsProperty = false;
        }

        /// <summary>
        /// Privilege name that must be checked for authorization
        /// </summary>
        public Privilege Privilege { get; set; }

        /// <summary>
        /// Index of the argument/parameter where dataObjectId is located. 
        /// Supported values are Guid, and BaseObject that contains key as Id 
        /// </summary>
        public int ObjectIdArgumentIndex { get; set; }

        /// <summary>
        /// Flag that the attribute is set on a property.
        /// </summary>
        public bool IsProperty { get; set; }
    }
}
