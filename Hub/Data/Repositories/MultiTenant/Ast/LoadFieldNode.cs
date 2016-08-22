namespace Data.Repositories.MultiTenant.Ast
{
    public class LoadFieldNode : AstNode
    {
        public int PropertyIndex;

        public LoadFieldNode(int propertyIndex)
        {
            PropertyIndex = propertyIndex;
        }
    }
}