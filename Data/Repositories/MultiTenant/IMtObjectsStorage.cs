using System.Collections.Generic;
using Data.Repositories.MultiTenant.Ast;

namespace Data.Repositories.MultiTenant
{
    interface IMtObjectsStorage
    {
        int Insert(string fr8AccountId, MtObject obj, AstNode uniqueConstraint);
        int Upsert(string fr8AccountId, MtObject obj, AstNode where);
        int Update(string fr8AccountId, MtObject obj, AstNode where);
        IEnumerable<MtObject> Query (string fr8AccountId, MtTypeDefinition type, AstNode where);
        int Delete(string fr8AccountId, MtTypeDefinition type, AstNode where);
    }
}