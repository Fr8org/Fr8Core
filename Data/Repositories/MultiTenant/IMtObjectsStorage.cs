using System.Collections.Generic;
using Data.Repositories.MultiTenant.Ast;

namespace Data.Repositories.MultiTenant
{
    interface IMtObjectsStorage
    {
        int Insert(IMtConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode uniqueConstraint);
        int Upsert(IMtConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode where);
        int Update(IMtConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode where);
        IEnumerable<MtObject> Query (IMtConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where);
        int Delete(IMtConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where);
    }
}