using System.Linq.Expressions;

namespace Data.Expressions
{
    /// <summary>
    ///  This class is used from replacing parameters in expressions.
    /// If we have two expression x => x == 5  and y => y > 50 we can't combine them into one because they use two different parameters 'x' and 'y'.
    /// To do so we have to use one common paramter in both expressions.
    /// </summary>
    internal class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _source ? _target : base.VisitParameter(node);
        }
    }
}