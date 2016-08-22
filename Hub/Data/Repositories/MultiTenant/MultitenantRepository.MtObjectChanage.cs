using Data.Repositories.MultiTenant.Ast;

namespace Data.Repositories.MultiTenant
{
    partial class MultitenantRepository
    {
        class MtObjectChange
        {
            public readonly MtObject Object;
            public readonly AstNode Constraint;
            public readonly MtObjectChangeType Type;
            public readonly string Fr8AccountId;

            public MtObjectChange(MtObject o, MtObjectChangeType type, AstNode constraint, string fr8AccountId)
            {
                Object = o;
                Type = type;
                Constraint = constraint;
                Fr8AccountId = fr8AccountId;
            }
        }
    }
}
