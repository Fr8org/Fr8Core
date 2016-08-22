namespace Data.Repositories.MultiTenant.Ast
{
    public class LoadConstNode : AstNode
    {
        public object Value;

        public LoadConstNode(object value)
        {
            Value = value;
        }
    }
}