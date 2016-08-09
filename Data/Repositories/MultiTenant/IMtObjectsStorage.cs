using System;
using System.Collections.Generic;
using Data.Repositories.MultiTenant.Ast;
using Data.Repositories.SqlBased;

namespace Data.Repositories.MultiTenant
{
    interface IMtObjectsStorage
    {
        int Insert(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode uniqueConstraint);
        int Upsert(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode where);
        int Update(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode where);
        IEnumerable<MtObject> Query (ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where);
        int QueryScalar (ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where);
        int Delete(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where);

        Guid? GetObjectId(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where);
    }
}