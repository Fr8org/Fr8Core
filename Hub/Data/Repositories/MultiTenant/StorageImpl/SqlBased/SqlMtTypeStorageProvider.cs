using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Data.Repositories.SqlBased;
using Fr8.Infrastructure.Data.Crates;

namespace Data.Repositories.MultiTenant.Sql
{
    class SqlMtTypeStorageProvider : IMtTypeStorageProvider
    {
        private readonly IMtTypeStorage _storage;
        private readonly List<MtTypeDefinition> _newTypes = new List<MtTypeDefinition>();
        // private readonly string _connectionString;

        public SqlMtTypeStorageProvider(IMtTypeStorage storage)
        {
            _storage = storage;
        }

        private SqlConnection OpenConnection(ISqlConnectionProvider connectionProvider)
        {
            var connection = new SqlConnection((string)connectionProvider.ConnectionInfo);

            connection.Open();
            return connection;
        }
        
        private MtTypeDefinition ReadType(SqlDataReader reader, string typeIdField, Type clrType)
        {
            MtTypeDefinition mtType;
            var id = (Guid)reader[typeIdField];
            var isComplex = (bool)reader["IsComplex"];
            var isPrimitive = (bool)reader["IsPrimitive"];

            if (isComplex)
            {
                mtType = MtTypeDefinition.MakeComplexType(id, clrType);
            }
            else if (isPrimitive)
            {
                mtType = MtTypeDefinition.MakePrimitive(id, clrType);
            }
            else
            {
                mtType = MtTypeDefinition.MakeType(id, clrType);
            }

            return mtType;
        }

        public IEnumerable<MtTypeReference> ListTypeReferences(ISqlConnectionProvider connectionProvider)
        {
            const string loadTypeReferences = "select * from MtTypes";

            using (var connection = OpenConnection(connectionProvider))
            using (var command = new SqlCommand(loadTypeReferences, connection))
            {
                var typeReferences = new List<MtTypeReference>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fullTypeName = reader["ClrName"];

                        if (fullTypeName == DBNull.Value)
                        {
                            continue;
                        }

                        var type = Type.GetType((string) fullTypeName);

                        if (type == null)
                        {
                            continue;
                        }

                        var mtType = ReadType(reader, "Id", type);
                        typeReferences.Add(new MtTypeReference(mtType.Alias, mtType.ClrType, mtType.Id));
                    }
                }

                return typeReferences;
            }
        }

        public IEnumerable<MtTypePropertyReference> ListTypePropertyReferences(ISqlConnectionProvider connectionProvider, Guid typeId)
        {
            using (var connection = OpenConnection(connectionProvider))
            using (var loadPropCommand = new SqlCommand(@"select  MtProperties.*, ptype.IsPrimitive, ptype.IsComplex, pType.ClrName, pType.ManifestId from MtProperties 
                                                                   inner join MtTypes as ptype on ptype.Id = MtProperties.Type      
                                                                   where MtProperties.DeclaringType = @declaringType"))
            {
                loadPropCommand.Connection = connection;

                loadPropCommand.Parameters.AddWithValue("@declaringType", typeId);

                var propertyReferences = new List<MtTypePropertyReference>();

                using (var reader = loadPropCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var propName = (string) reader["Name"];
                        var fullTypeName = reader["ClrName"];

                        if (fullTypeName == DBNull.Value)
                        {
                            continue;
                        }

                        var type = Type.GetType((string) fullTypeName);

                        if (type == null)
                        {
                            continue;
                        }

                        var propType = ReadType(reader, "Type", type);
                        var index = (int) reader["Offset"];

                        propertyReferences.Add(new MtTypePropertyReference(typeId, type, propType.Id, propName, index));



                    }
                }

                return propertyReferences;
            }
        }

        public MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, Type type)
        {
            MtTypeDefinition typeDef;

            if (!TryLoadType(connectionProvider, type, out typeDef))
            {
                return null;
            }

            return new MtTypeReference(typeDef.Alias, typeDef.ClrType, typeDef.Id);
        }

        public MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, Guid typeId)
        {
            const string loadTypeReferences = "select * from MtTypes where Id = @id";

            using (var connection = OpenConnection(connectionProvider))
            using (var command = new SqlCommand(loadTypeReferences, connection))
            {
                command.Parameters.AddWithValue("@id", typeId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fullTypeName = reader["ClrName"];

                        if (fullTypeName == DBNull.Value)
                        {
                            continue;
                        }

                        var type = Type.GetType((string)fullTypeName);

                        if (type == null)
                        {
                            continue;
                        }

                        var mtType = ReadType(reader, "Id", type);
                        return new MtTypeReference(mtType.Alias, mtType.ClrType, mtType.Id);
                    }
                }

                return null;
            }
        }

        public MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, string alias)
        {
            const string loadTypeReferences = "select * from MtTypes where Alias = @alias";

            using (var connection = OpenConnection(connectionProvider))
            using (var command = new SqlCommand(loadTypeReferences, connection))
            {
                command.Parameters.AddWithValue("@alias", alias);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fullTypeName = reader["ClrName"];

                        if (fullTypeName == DBNull.Value)
                        {
                            continue;
                        }

                        var type = Type.GetType((string)fullTypeName);

                        if (type == null)
                        {
                            continue;
                        }

                        var mtType = ReadType(reader, "Id", type);
                        return new MtTypeReference(mtType.Alias, mtType.ClrType, mtType.Id);
                    }
                }

                return null;
            }
        }

        // Currently we don't need a support for object properties that have type of 'object'.
        // All our properties are of primitive types (types that has no own properties) and  we don't want to introduce unnecessary complications related to types graphs
        // So we make assumption that we can always safely load the type with all possible references by loading only topmost level of type hierarchy
        // This is as easy as making one sql join.
        public bool TryLoadType(ISqlConnectionProvider connectionProvider, Type clrType, out MtTypeDefinition mtType)
        {
            // we want handle Manifests in the special way. Instead of storing manifest name as CLR type name we'll store it as manifest id.
            // this will make out system more robust because it will not depend on manifest names.
            CrateManifestType manifestType;
            mtType = null;

            using (var connection = OpenConnection(connectionProvider))
            {
                SqlCommand loadTypeCommand;
                
                // search type by manifest Id
                if (ManifestDiscovery.Default.TryGetManifestType(clrType, out manifestType))
                {
                    loadTypeCommand = new SqlCommand(@"select top 1 * from MtTypes where ManifestId = @manifestId")
                    {
                        Connection = connection
                    };

                    loadTypeCommand.Parameters.AddWithValue("@manifestId", manifestType.Id);
                }
                else
                {
                    loadTypeCommand = new SqlCommand(@"select top 1 * from MtTypes where ClrName = @clrType")
                    {
                        Connection = connection
                    };

                    loadTypeCommand.Parameters.AddWithValue("@clrType", clrType.AssemblyQualifiedName);
                }

                using (var reader = loadTypeCommand.ExecuteReader())
                {
                    // read type
                    while (reader.Read())
                    {
                        mtType = ReadType(reader, "Id", clrType);
                        break;
                    }

                    // we didn't find type
                    if (mtType == null)
                    {
                        return false;
                    }
                }

                if (!mtType.IsComplexType && !mtType.IsPrimitive)
                {
                    using (var loadPropCommand = new SqlCommand(@"select  MtProperties.*, ptype.IsPrimitive, ptype.IsComplex, pType.ClrName, pType.ManifestId from MtProperties 
                                                                   inner join MtTypes as ptype on ptype.Id = MtProperties.Type      
                                                                   where MtProperties.DeclaringType = @declaringType order by Offset"))
                    {
                        loadPropCommand.Connection = connection;

                        loadPropCommand.Parameters.AddWithValue("@declaringType", mtType.Id);

                        using (var reader = loadPropCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var propName = (string)reader["Name"];
                                var fullTypeName = reader["ClrName"];

                                if (fullTypeName == DBNull.Value)
                                {
                                    continue;
                                }

                                var type = Type.GetType((string) fullTypeName);

                                if (type == null)
                                {
                                    continue;
                                }

                                var propType = ReadType(reader, "Type", type);
                                var index = (int) reader["Offset"];
                                mtType.Properties.Add(new MtPropertyInfo(index, propName, mtType, propType));
                            }
                        }

                        for (int i = 0; i < mtType.Properties.Count; i++)
                        {
                            if (mtType.Properties[i].Index != i)
                            {
                                throw new Exception(string.Format("MtProperties table is corrupted for typeId '{0}' [ClrType: {1}]", mtType.Id, clrType.FullName));
                            }
                        }
                    }
                }
            }

            return true;
        }

        public void PersistType(ISqlConnectionProvider connectionProvider, MtTypeDefinition mtType)
        {
            _newTypes.Add(mtType);
        }

        public void SaveChanges(ISqlConnectionProvider connectionProvider)
        {
            const string insertTypeCommand = @"insert into MtTypes (Id, Alias, ClrName, IsPrimitive, IsComplex, ManifestId) values (@typeId, @alias, @clrName, @isPrimitive, @isComplex, @manifestId)";
            const string insertPropertyCommand = @"insert into MtProperties (Name, Offset, Type, DeclaringType) values (@name, @offset, @type, @declaringType)";

            using (var typeLock = _storage.AccureTypeTransactionLock())
            {
                using (var connection = OpenConnection(connectionProvider))
                using (var transaction = connection.BeginTransaction())
                using (var insertType = new SqlCommand(insertTypeCommand))
                using (var insertProperty = new SqlCommand(insertPropertyCommand))
                {
                    insertType.Transaction = transaction;
                    insertType.Connection = connection;
                    insertType.Parameters.Add("@typeId", SqlDbType.UniqueIdentifier);
                    insertType.Parameters.Add("@alias", SqlDbType.NVarChar);
                    insertType.Parameters.Add("@clrName", SqlDbType.NVarChar);
                    insertType.Parameters.Add("@isPrimitive", SqlDbType.Bit);
                    insertType.Parameters.Add("@isComplex", SqlDbType.Bit);
                    insertType.Parameters.Add("@manifestId", SqlDbType.Int);

                    insertProperty.Transaction = transaction;
                    insertProperty.Connection = connection;
                    insertProperty.Parameters.Add("@name", SqlDbType.NVarChar);
                    insertProperty.Parameters.Add("@offset", SqlDbType.Int);
                    insertProperty.Parameters.Add("@type", SqlDbType.UniqueIdentifier);
                    insertProperty.Parameters.Add("@declaringType", SqlDbType.UniqueIdentifier);

                    foreach (var mtTypeDefinition in _newTypes)
                    {
                        if (!typeLock.IsTypePersisted(mtTypeDefinition.Id))
                        {
                            // we want handle Manifests in the special way. Instead of storing manifest name as CLR type name we'll store it as manifest id.
                            // this will make out system more robust because it will not depend on manifest names.
                            CrateManifestType manifestType;

                            if (ManifestDiscovery.Default.TryGetManifestType(mtTypeDefinition.ClrType, out manifestType))
                            {
                                insertType.Parameters["@manifestId"].Value = manifestType.Id;
                            }
                            else
                            {
                                insertType.Parameters["@manifestId"].Value = DBNull.Value;
                            }

                            insertType.Parameters["@typeId"].Value = mtTypeDefinition.Id;
                            insertType.Parameters["@alias"].Value = mtTypeDefinition.Alias ?? (object)DBNull.Value;
                            insertType.Parameters["@clrName"].Value = mtTypeDefinition.ClrType.AssemblyQualifiedName;
                            insertType.Parameters["@isPrimitive"].Value = mtTypeDefinition.IsPrimitive;
                            insertType.Parameters["@isComplex"].Value = mtTypeDefinition.IsComplexType;
                            insertType.ExecuteNonQuery();

                            if (mtTypeDefinition.Properties != null)
                            {
                                foreach (var mtPropertyInfo in mtTypeDefinition.Properties)
                                {
                                    insertProperty.Parameters["@name"].Value = mtPropertyInfo.Name;
                                    insertProperty.Parameters["@offset"].Value = mtPropertyInfo.Index;
                                    insertProperty.Parameters["@type"].Value = mtPropertyInfo.MtPropertyType.Id;
                                    insertProperty.Parameters["@declaringType"].Value = mtPropertyInfo.DeclaringType.Id;
                                    insertProperty.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }

                typeLock.Commit(_newTypes.Select(x => x.Id));
                _newTypes.Clear();
            }
        }
    }
}
 