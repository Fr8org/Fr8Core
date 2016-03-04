using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Repositories.MultiTenant.Ast;

namespace Data.Repositories.MultiTenant
{
    static class ExpressionToAstConverter
    {
        public static AstNode Convert(Expression expression, MtTypeDefinition type)
        {
            if (expression == null)
            {
                return null;
            }

            var visitor = new AstConverterExpressionVisitor(type);

            visitor.Visit(expression);

            return visitor.GetRoot();
        }
    }

    class AstConverterExpressionVisitor : ExpressionVisitor
    {
        private readonly MtTypeDefinition _type;
        private readonly Stack<AstNode> _stack = new Stack<AstNode>();

        public AstConverterExpressionVisitor(MtTypeDefinition type)
        {
            _type = type;
        }

        public AstNode GetRoot()
        {
            return _stack.Pop();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            MtBinaryOp op;

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    op = MtBinaryOp.Eq;
                    break;

                case ExpressionType.NotEqual:
                    op = MtBinaryOp.Neq;
                    break;

                case ExpressionType.GreaterThan:
                    op = MtBinaryOp.Gt;
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    op = MtBinaryOp.Gte;
                    break;

                case ExpressionType.LessThan:
                    op = MtBinaryOp.Lt;
                    break;

                case ExpressionType.LessThanOrEqual:
                    op = MtBinaryOp.Lte;
                    break;

                case ExpressionType.AndAlso:
                    op = MtBinaryOp.And;
                    break;

                case ExpressionType.OrElse:
                    op = MtBinaryOp.Or;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Unsupported operation: " + node.NodeType);
            }

            var ast = new BinaryOpNode(op);

            Visit(node.Left);
            Visit(node.Right);

            ast.Right = _stack.Pop();
            ast.Left = _stack.Pop();

            _stack.Push(ast);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _stack.Push(new LoadConstNode(node.Value));
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _stack.Push(new LoadConstNode(GetValue(node)));
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expr = node;
            // check what object current accessor is belong to.
            // if current accessor doesn't belong to lambda's paramter expression i.e x.EnvelopeId it is likely to be some captured variable or static property.
            // try to compile and execute this expression then!
            while(true)
            {
                if (expr.Expression is MemberExpression)
                {
                    expr = (MemberExpression) expr.Expression;
                }
                else
                {
                    if (expr.Expression == null || expr.Expression.NodeType != ExpressionType.Parameter)
                    {
                        _stack.Push(new LoadConstNode(GetValue(node)));
                        return node;
                    }

                    break;
                }
            }

            expr = node;
            // if we see nullable type in member access like x.SomeDate.Value
            // we want to skip that 'Value' property accessor. 
            if (node.Member.DeclaringType != null && node.Member.DeclaringType.IsGenericType && node.Member.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                while (expr.Expression is MemberExpression)
                {
                    expr = (MemberExpression)expr.Expression;
                }
            }

            var mtProp = _type.Properties.FirstOrDefault(x => x.Name == expr.Member.Name);

            if (mtProp == null)
            {
                throw new Exception(string.Format("Property {0} is not found in MT type definition", expr.Member.Name));
            }

            _stack.Push(new LoadFieldNode(mtProp.Index));

            return node;
        }

        private object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}
