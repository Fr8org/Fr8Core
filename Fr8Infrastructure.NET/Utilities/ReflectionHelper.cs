using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fr8.Infrastructure.Utilities
{
    public class ReflectionHelper<TOnType>
    {
        //This creates a statically typed reference to our supplied property. If we change it in the future, it won't compile (so it won't break at runtime).
        //Changing the property with tools like resharper will automatically update here.
        public string GetPropertyName<TReturnType>(Expression<Func<TOnType, TReturnType>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
                return (expression.Body as dynamic).Member.Name;

            throw new Exception("Cannot contain complex expressions. An example of a supported expression is 'ev => ev.Id'");
        }

        public PropertyInfo GetProperty<TReturnType>(Expression<Func<TOnType, TReturnType>> expression)
        {
            if (expression.Body is MemberExpression)
                if ((expression.Body as MemberExpression).Member is PropertyInfo)
                    return (expression.Body as MemberExpression).Member as PropertyInfo;

            throw new Exception("Not a property");
        }

    }

    public class ReflectionHelper
    {
        public static PropertyInfo EntityPrimaryKeyPropertyInfo(object entity)
        {
            Type entityType;
            if (entity is Type)
                entityType = (Type) entity;
            else
                entityType = entity.GetType();

            return entityType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
        }

        public static PropertyInfo ForeignKeyNavitationProperty(object entity, PropertyInfo foreignKeyProperty)
        {
            var foreignKeyAttribute = foreignKeyProperty.GetCustomAttribute<ForeignKeyAttribute>();
            if (foreignKeyAttribute == null)
                return null;
            var navigationProperty = entity.GetType().GetProperty(foreignKeyAttribute.Name);
            return navigationProperty;
        }

        public static PropertyInfo[] GetProperties(Type curType)
        {
            var curProperties = curType.GetProperties();
            return curProperties;
        }
    }
}
