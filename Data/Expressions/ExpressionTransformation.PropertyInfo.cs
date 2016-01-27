using System;

namespace Data.Expressions
{
    partial class ExpressionTransformation<TSource, TTarget>
    {
        // Information about the property from the expression
        // This class has overriden equality members and can be used in HashSet to get list of unique properties
        public class PropertyInfo
        {
            /**********************************************************************************/
            // Declarations
            /**********************************************************************************/

            public readonly string Name;
            public readonly Type Type;

            /**********************************************************************************/
            // Functions
            /**********************************************************************************/

            public PropertyInfo(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            /**********************************************************************************/

            protected bool Equals(PropertyInfo other)
            {
                return String.Equals(Name, (string) other.Name);
            }

            /**********************************************************************************/

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PropertyInfo)obj);
            }

            /**********************************************************************************/

            public override int GetHashCode()
            {
                return (Name != null ? Name.GetHashCode() : 0);
            }

            /**********************************************************************************/
        }
    }
}
