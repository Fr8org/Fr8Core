using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Data.Repositories.MultiTenant.Ast;
using Data.Repositories.SqlBased;

namespace Data.Repositories.MultiTenant.Sql
{
    public partial class SqlMtObjectsStorage : IMtObjectsStorage
    {
        private readonly string _connectionString;
        private readonly IMtObjectConverter _converter;

        public SqlMtObjectsStorage(IMtObjectConverter converter)
        {
            _converter = converter;
        }

        private SqlConnection OpenConnection(ISqlConnectionProvider connectionProvider)
        {
            var connection = new SqlConnection((string)connectionProvider.ConnectionInfo);

            connection.Open();
            return connection;
        }

        private int Upsert(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode @where, bool allowUpdate, bool allowInsert)
        {
            var fields = new List<string>
            {
                "Type",
                "CreatedAt",
                "UpdatedAt",
                "fr8AccountId",
                "IsDeleted"
            };

            var parameters = new List<string>
            {
                "@type",
                "@created",
                "@updated",
                "@account",
                "@isDeleted"
            };

            foreach (var mtPropertyInfo in obj.MtTypeDefinition.Properties)
            {
                parameters.Add("@val" + (mtPropertyInfo.Index + 1));
                fields.Add("Value" + (mtPropertyInfo.Index + 1));
            }

            var tableDefintion = string.Join(", ", fields);

            using (var connection = OpenConnection(connectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@type", obj.MtTypeDefinition.Id);
                    command.Parameters.AddWithValue("@created", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@account", fr8AccountId);
                    command.Parameters.AddWithValue("@isDeleted", false);

                    foreach (var mtPropertyInfo in obj.MtTypeDefinition.Properties)
                    {
                        var value = obj.Values[mtPropertyInfo.Index];
                        object dbValue = DBNull.Value;

                        if (value != null)
                        {
                            dbValue = value;
                        }

                        command.Parameters.AddWithValue("@val" + (mtPropertyInfo.Index + 1), dbValue);
                    }

                    if (@where != null)
                    {
                        var valuesToInsert = string.Join(", ", fields.Select(x => "Src." + x));
                        var astConverter = new AstToSqlConverter(obj.MtTypeDefinition, _converter, "Tgt");

                        astConverter.Convert(@where);

                        var cmd = string.Format(@"merge MtData as Tgt 
                                               using (select {0}) as Src ({1}) 
                                               ON Tgt.Type = @type and Tgt.fr8AccountId = @account and ({2}) and Tgt.IsDeleted = 0", string.Join(",", parameters), tableDefintion, astConverter.SqlCommand);

                        if (allowUpdate)
                        {
                            cmd += string.Format("\nwhen matched then update set {0}", string.Join(", ", fields.Where(x => x != "CreatedAt").Select(x => string.Format("Tgt.{0} = Src.{0}", x))));
                        }

                        if (allowInsert)
                        {
                            cmd += string.Format("\nwhen not matched then insert ({0}) values ({1});", tableDefintion, valuesToInsert);
                        }

                        command.CommandText = cmd;

                        for (int index = 0; index < astConverter.Constants.Count; index++)
                        {
                            command.Parameters.AddWithValue("@param" + index, astConverter.Constants[index]);
                        }
                    }
                    else
                    {
                        if (!allowInsert)
                        {
                            return 0;
                        }

                        command.CommandText = string.Format(@"insert into MtData ({0}) values ({1})", tableDefintion, string.Join(",", parameters));
                    }

                    var affectedRows = command.ExecuteNonQuery();



                    return affectedRows;
                }
            }
        }

        public int Upsert(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode @where)
        {
            return Upsert(connectionProvider, fr8AccountId, obj, @where, true, true);
        }

        public int Insert(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode uniqueConstraint)
        {
            var affectedRows = Upsert(connectionProvider, fr8AccountId, obj, uniqueConstraint, false, true);

            if (affectedRows == 0)
            {
                throw new Exception("Violation of unique constraint");
            }

            return affectedRows;
        }

        public int Update(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject obj, AstNode @where)
        {
            return Upsert(connectionProvider, fr8AccountId, obj, @where, true, false);
        }

        public int QueryScalar(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode @where)
        {
            using (var connection = OpenConnection(connectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@type", type.Id);
                    command.Parameters.AddWithValue("@account", fr8AccountId);
                    command.Parameters.AddWithValue("@isDeleted", false);

                    string whereTemplate = string.Empty;

                    if (where != null)
                    {
                        var astConverter = new AstToSqlConverter(type, _converter, "[md]");

                        astConverter.Convert(@where);

                        whereTemplate = " and " + astConverter.SqlCommand;

                        for (int index = 0; index < astConverter.Constants.Count; index++)
                        {
                            command.Parameters.AddWithValue("@param" + index, astConverter.Constants[index]);
                        }
                    }

                    command.CommandText = string.Format(MtSelectScalarTemplate, whereTemplate);

                    return (int)Convert.ChangeType(command.ExecuteScalar(), typeof(int));
                }
            }
        }

        public IEnumerable<MtObject> Query(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode @where)
        {
            var fields = new List<string>
            {
                "Type",
                "fr8AccountId",
                "IsDeleted"
            };

            var parameters = new List<string>
            {
                "@type",
                "@account",
                "@isDeleted"
            };

            foreach (var mtPropertyInfo in type.Properties)
            {
                parameters.Add("@val" + (mtPropertyInfo.Index + 1));
                fields.Add("Value" + (mtPropertyInfo.Index + 1));
            }

            var tableDefintionOuter = string.Join(", ", fields.Select(x => "tmp." + x));
            var tableDefintionInner = string.Join(", ", fields.Select(x => "[md]." + x));

            using (var connection = OpenConnection(connectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@type", type.Id);
                    command.Parameters.AddWithValue("@account", fr8AccountId);
                    command.Parameters.AddWithValue("@isDeleted", false);

                    string whereTemplate = string.Empty;

                    if (where != null)
                    {
                        var astConverter = new AstToSqlConverter(type, _converter, "[md]");

                        astConverter.Convert(@where);

                        whereTemplate = " and " + astConverter.SqlCommand;

                        for (int index = 0; index < astConverter.Constants.Count; index++)
                        {
                            command.Parameters.AddWithValue("@param" + index, astConverter.Constants[index]);
                        }
                    }

                    command.CommandText = string.Format(MtSelectQueryTemplate, tableDefintionOuter, tableDefintionInner, whereTemplate);
                    command.CommandTimeout = 120;

                    var result = new List<MtObject>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var obj = new MtObject(type);

                            foreach (var mtPropertyInfo in type.Properties)
                            {
                                var val = reader["Value" + (mtPropertyInfo.Index + 1)];

                                if (val != DBNull.Value)
                                {
                                    obj.Values[mtPropertyInfo.Index] = (string)val;
                                }
                            }

                            result.Add(obj);
                        }
                    }

                    return result;
                }
            }
        }

        public int Delete(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode @where)
        {
            if (where == null)
            {
                throw new ApplicationException("Where clause must be provided.");
            }

            using (var connection = OpenConnection(connectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    var astConverter = new AstToSqlConverter(type, _converter);
                    astConverter.Convert(where);

                    var sqlCommand = @"delete FROM [dbo].[MtData] WHERE fr8AccountId = @accountId and Type = @type and " + astConverter.SqlCommand;

                    command.Parameters.AddWithValue("@type", type.Id);
                    command.Parameters.AddWithValue("@accountId", fr8AccountId);

                    command.CommandText = sqlCommand;

                    for (int index = 0; index < astConverter.Constants.Count; index++)
                    {
                        command.Parameters.AddWithValue("@param" + index, astConverter.Constants[index]);
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        public Guid? GetObjectId(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where)
        {
            if (where == null)
            {
                throw new ApplicationException("Where clause must be provided.");
            }

            using (var connection = OpenConnection(connectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    var astConverter = new AstToSqlConverter(type, _converter);
                    astConverter.Convert(where);

                    var sqlCommand = @"SELECT [Id] FROM [dbo].[MtData] WHERE " + astConverter.SqlCommand;
                    command.CommandText = sqlCommand;

                    for (int index = 0; index < astConverter.Constants.Count; index++)
                    {
                        command.Parameters.AddWithValue("@param" + index, astConverter.Constants[index]);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }
                        return reader.GetGuid(0);
                    }
                }
            }
        }
    }
}
