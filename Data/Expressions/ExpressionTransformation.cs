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
    // Example of usage:
    // Suppose we have the following Manifest with properties 'Name' and 'Id'. When this manifest is stored in MT DB 
    // property 'Name' maps to MT_Data property 'Value1' and 'Id' maps to 'Value2'
    // we have expression against our manifest:
    // x => x.Name == "sample name" && Id == "2"
    // by using ExpressionTransformation we can get equivalent expression against MT_Data:
    // x => x.Value1 == "sample name" && x.Value2 == "2"
    public partial class ExpressionTransformation<TSource, TTarget>
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly Func<string, string> _propertyNamesMapper;
        private readonly HashSet<PropertyInfo> _properties = new HashSet<PropertyInfo>();

        /**********************************************************************************/
        // List of TSoruce properties from the expression
        public PropertyInfo[] Properties
        {
            get { return _properties.ToArray(); }
        }

        /**********************************************************************************/
        /// <summary>
        /// Predicate against TTarget. This predicate can be used to filter instances of TTarget 
        /// using criterias from expression against TSource that was passed to Parse method
        /// </summary>
        public Func<TTarget, bool> CompiledTargetExpression
        {
            get; 
            private set;
        }

        /**********************************************************************************/
        /// <summary>
        /// Expression against TTarget
        /// </summary>
        public Expression<Func<TTarget, bool>> TargetExpression
        {
            get;
            private set;
        }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyNamesMapper">set mapping from TSource propertys to TTarget properties</param>
        public ExpressionTransformation(Func<string, string> propertyNamesMapper)
        {
            _propertyNamesMapper = propertyNamesMapper;
        }

        /**********************************************************************************/
        /// <summary>
        /// Parse expression and convert it to exspression against TTarget
        /// </summary>
        /// <param name="expression"></param>
        public void Parse(Expression<Func<TSource, bool>> expression)
        {
            _properties.Clear();
            TargetExpression = null;
            CompiledTargetExpression = null;

            var p = Expression.Parameter(typeof(TTarget), expression.Parameters[0].Name);
            var v = new TransformationVisitor(p, _propertyNamesMapper, _properties);
            var newBody = v.Visit(expression.Body);

            TargetExpression = Expression.Lambda<Func<TTarget, bool>>(newBody, p);
            CompiledTargetExpression = TargetExpression.Compile();
        }

        /**********************************************************************************/
    }
}