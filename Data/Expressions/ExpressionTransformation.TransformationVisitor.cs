using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Data.Expressions
{
    partial class ExpressionTransformation<TSource, TTarget>
    {
        // Replaces TSource property acessors with TTarget property expressions with the corresponding type conversions
        private class TransformationVisitor : ExpressionVisitor
        {
            /**********************************************************************************/
            //  Declarations
            /**********************************************************************************/

            private readonly ParameterExpression _paramExpr;
            private readonly Func<string, string> _propNamesMapper;
            private readonly HashSet<PropertyInfo> _discoveredProperties;

            /**********************************************************************************/
            // Functions
            /**********************************************************************************/

            public TransformationVisitor(ParameterExpression paramExpr, Func<string, string> propNamesMapper, HashSet<PropertyInfo> discoveredProperties)
            {
                _discoveredProperties = discoveredProperties;
                _paramExpr = paramExpr;
                _propNamesMapper = propNamesMapper;
            }

            /**********************************************************************************/

            private Expression GetMtPropertyExpression(string propertyName, Type targetType, ParameterExpression pe)
            {
                var mtProp = _propNamesMapper(propertyName);

                if (targetType == typeof(string))
                {
                    return Expression.Property(pe, mtProp);
                }

                return Expression.Convert(Expression.Call(ConversionMethods.GenericChangeTypeMethodInfo, new Expression[] { Expression.Property(pe, mtProp), Expression.Constant(targetType), Expression.Constant(CultureInfo.InvariantCulture) }), targetType);
            }

            /**********************************************************************************/

            public override Expression Visit(Expression node)
            {
                if (node.NodeType == ExpressionType.MemberAccess)
                {
                    var expr = (MemberExpression)node;
                    Type propType;

                    if (expr.Member is System.Reflection.PropertyInfo)
                    {
                        propType = ((System.Reflection.PropertyInfo)expr.Member).PropertyType;
                    }
                    else if (expr.Member is FieldInfo)
                    {
                        propType = ((FieldInfo)expr.Member).FieldType;
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Member {0} has unsupported member type {1}", expr.Member.Name, expr.Member.GetType().Name));
                    }

                    _discoveredProperties.Add(new PropertyInfo(expr.Member.Name, propType));

                    return GetMtPropertyExpression(expr.Member.Name, propType, _paramExpr);
                }

                return base.Visit(node);
            }

            /**********************************************************************************/
        }
    }
}
