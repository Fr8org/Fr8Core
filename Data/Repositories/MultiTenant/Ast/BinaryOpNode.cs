namespace Data.Repositories.MultiTenant.Ast
{
    public class BinaryOpNode : AstNode
    {
        public MtBinaryOp Op;
        public AstNode Left;
        public AstNode Right;

        public BinaryOpNode(MtBinaryOp op)
        {
            Op = op;
        }
    }
}