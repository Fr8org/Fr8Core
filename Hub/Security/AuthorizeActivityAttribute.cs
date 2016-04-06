using System;
using Data.States;

namespace Hub.Security
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizeActivityAttribute : Attribute
    {
        public AuthorizeActivityAttribute()
        {
            ObjectIdArgumentIndex = 0;
            Privilege = Privileges.ReadObject;
        }
        public string Privilege { get; set; }

        public int ObjectIdArgumentIndex { get; set; }
    }
}
