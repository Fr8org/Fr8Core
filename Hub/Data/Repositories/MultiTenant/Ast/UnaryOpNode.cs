namespace Data.Repositories.MultiTenant.Ast
{
    public class UnaryOpNode : AstNode
    {
        public MtUnaryOp Op;
        public AstNode Node;
    }
}