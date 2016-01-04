using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Expressions
{
    // This class is used to transform expressions written for Manifest objects into expressions for MT_Data.
    // This is done be replacing property names and applying corresponding type casting. Currently we cast MT_Data string into 
    // the coresponding Manifest property type. Casting is necessary to make comparison work.
    // In future this class with some modifications can be used to produce expressions suitable for Linq-to-Sql.
    public partial class ExpressionTransformation<TSource, TTarget>
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly Func<string, string> _propertyNamesMapper;
        private readonly HashSet<PropertyInfo> _properties = new HashSet<PropertyInfo>();

        /**********************************************************************************/

        public PropertyInfo[] Properties
        {
            get { return _properties.ToArray(); }
        }

        /**********************************************************************************/

        public Func<TTarget, bool> CompiledTargetExpression
        {
            get; 
            private set;
        }

        /**********************************************************************************/

        public Expression<Func<TTarget, bool>> TargetExpression
        {
            get;
            private set;
        }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public ExpressionTransformation(Func<string, string> propertyNamesMapper)
        {
            _propertyNamesMapper = propertyNamesMapper;
        }

        /**********************************************************************************/

        public void Parse(Expression<Func<TSource, bool>> expression)
        {
            _properties.Clear();
            TargetExpression = null;
            CompiledTargetExpression = null;

            var p = Expression.Parameter(typeof(TTarget), expression.Parameters[0].Name);
            var v = new TransfromationVisitor(p, _propertyNamesMapper, _properties);
            var newBody = v.Visit(expression.Body);

            TargetExpression = Expression.Lambda<Func<TTarget, bool>>(newBody, p);
            CompiledTargetExpression = TargetExpression.Compile();
        }

        /**********************************************************************************/
    }
}