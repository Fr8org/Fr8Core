using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories.Security.Entities;
using Data.Repositories.SqlBased;
using Data.States;
using Data.States.Templates;
using Fr8Data.DataTransferObjects;

namespace Data.Repositories.Security.StorageImpl.SqlBased
{
    /// <summary>
    /// Sql Storage Provider used to manipulate security objects. Not connected to EF Context due to Securuty Objects connection to AspNetRolesDO and their problem with caching.
    /// This Storage provider has loose coupling with SecurityObjectCache, where results from these methods here are being cached. Check SecurityObjectStorage
    /// </summary>
    public class SqlSecurityObjectsStorageProvider : ISecurityObjectsStorageProvider
    {
        private readonly string _connectionString;
        private readonly ISqlConnectionProvider _sqlConnectionProvider;
        private const string InsertRolePermissionCommand = "insert into dbo.RolePermission(id, permissionSetId, roleId, createDate, lastUpdated) values (@id, @permissionSetId, @roleId, @createDate, @lastUpdated)";
        private const string InsertObjectRolePermissionCommand = "insert into dbo.ObjectRolePermissions(objectId, rolePermissionId, type, propertyName, createDate, lastUpdated) values (@objectId, @rolePermissionId, @type, @propertyName, @createDate, @lastUpdated)";
        
        public SqlSecurityObjectsStorageProvider(ISqlConnectionProvider sqlConnectionProvider)
        {
            _sqlConnectionProvider = sqlConnectionProvider;
        }

        private SqlConnection OpenConnection(ISqlConnectionProvider connectionProvider)
        {
            var connection = new SqlConnection((string)connectionProvider.ConnectionInfo);

            connection.Open();

            return connection;
        }

        public int InsertRolePermission(RolePermission rolePermission)
        {
            var affectedRows = Upsert(_sqlConnectionProvider, rolePermission, false, true);

            if (affectedRows == 0)
            {
                throw new Exception("Violation of unique constraint");
            }

            return affectedRows;
        }

        public int UpdateRolePermission(RolePermission rolePermission)
        {
            return Upsert(_sqlConnectionProvider, rolePermission, true, false);
        }

        private int Upsert(ISqlConnectionProvider sqlConnectionProvider, RolePermission rolePermission, bool allowUpdate, bool allowInsert)
        {
            using (var connection = OpenConnection(sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    if (rolePermission.Id == Guid.Empty)
                    {
                        rolePermission.Id = Guid.NewGuid();
                        command.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                    }
                    
                    command.Parameters.AddWithValue("@id", rolePermission.Id);
                    command.Parameters.AddWithValue("@permissionSetId", rolePermission.PermissionSet.Id);
                    command.Parameters.AddWithValue("@roleId", rolePermission.Role.RoleId);
                    command.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                    var cmdText = String.Empty;
                    if (allowInsert)
                    {
                        cmdText = InsertRolePermissionCommand;
                    }

                    if (allowUpdate)
                    {
                        cmdText = "update dbo.RolePermissions set PermissionSetId = @permissionSetId, roleId = @roleId, lastUpdated = @lastUpdated";
                    }

                    command.CommandText = cmdText;
                    var affectedRows = command.ExecuteNonQuery();

                    return affectedRows;
                }
            }
        }


        public ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(string dataObjectId)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd =
                        @"select rp.Id, orp.PropertyName, orp.ObjectId as ObjectId, orp.Type, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate,
                            p.Id as PermissionSetId, p.ObjectType
                          from dbo.RolePermissions rp          
                          inner join dbo.PermissionSets p on rp.PermissionSetId = p.Id                                                                  
                          inner join dbo.ObjectRolePermissions orp on rp.Id = orp.RolePermissionId                               
                          inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id   ";

                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.CommandText = cmd;

                    var result = new ObjectRolePermissionsWrapper();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReadObjectRolePermissionFromSql(reader, result);
                        }
                    }

                    //fetch all permissions for ObjectRolePermission Sets
                    foreach (var item in result.RolePermissions)
                    {
                        var selectPermissionSetSql = "select Id from dbo.PermissionSetPermissions where PermissionSetId = @permissionSetId";

                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@permissionSetId", item.PermissionSet.Id);
                        command.CommandText = selectPermissionSetSql;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //this data is only for internal logic, cannot be saved
                                item.PermissionSet.Permissions.Add(new _PermissionTypeTemplate()
                                {
                                    Id = reader["Id"] != DBNull.Value ? (int) reader["Id"] : 0
                                });
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public List<PermissionDTO> GetAllPermissionsForUser(Guid profileId)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var permissionId = Guid.Empty;

                    using (var selectCommand = new SqlCommand())
                    {
                        selectCommand.Connection = connection;
                        selectCommand.Transaction = transaction;

                        //select all role permission for roleName OwnerOfCurrentObject
                        selectCommand.Parameters.AddWithValue("@roleName", Roles.OwnerOfCurrentObject);
                        selectCommand.Parameters.AddWithValue("@dataObjectType", dataObjectType);
                        selectCommand.CommandText = "select per.Id from dbo.Profiles p inner join dbo.AspNetRoles anr on p.Id = anr.ProfileIdinner join dbo.ProfilePermissions pp on p.Id = pp.ProfileId inner join dbo.Permissions per on per.Id = pp.PermissionId  where anr.Name = @roleName and per.Type = dataObjectType";

                        using (var reader = selectCommand.ExecuteReader())
                        {
                            reader.Read();
                            permissionId = (Guid) reader["Id"];

                            //todo: create default permission in case missing
                        }
                    }
                    
                    using (var insertCommand = new SqlCommand())
                    {
                        insertCommand.Connection = connection;
                        insertCommand.Transaction = transaction;

                        insertCommand.Parameters.Clear();
                        insertCommand.Parameters.AddWithValue("@objectId", dataObjectId);
                        insertCommand.Parameters.AddWithValue("@rolePermissionId", permissionId);
                        insertCommand.Parameters.AddWithValue("@type", dataObjectType);
                        insertCommand.Parameters.AddWithValue("@propertyName", DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                        insertCommand.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                        var cmdText = InsertObjectRolePermissionCommand;

                        insertCommand.CommandText = cmdText;
                        var affectedRows = insertCommand.ExecuteNonQuery();

                        if (affectedRows == 0)
                        {
                            throw new Exception("Problem with Inserting new ObjectRolePermission");
                        }
                    }
                    
                    transaction.Commit();
                }
            }
        }

        public int InsertObjectRolePermission(string dataObjectId, Guid rolePermissionId, string dataObjectType, string propertyName = null)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.Parameters.AddWithValue("@rolePermissionId", rolePermissionId);
                    command.Parameters.AddWithValue("@type", dataObjectType);
                    command.Parameters.AddWithValue("@propertyName", propertyName != null ? (object) propertyName : DBNull.Value);
                    command.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                    command.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                    var cmdText = InsertObjectRolePermissionCommand;
                    
                    command.CommandText = cmdText;
                    var affectedRows = command.ExecuteNonQuery();

                    if (affectedRows == 0)
                    {
                        throw new Exception("Problem with Inserting new ObjectRole Permission");
                    }

                    return affectedRows;
                }
            }
        }

        public int RemoveObjectRolePermission(string dataObjectId, Guid rolePermissionId, string propertyName = null)
        {
            throw new NotImplementedException();
        }

        public List<int> GetObjectBasedPermissionSetForObject(string dataObjectId, string dataObjectType, Guid profileId)
        {
            return new List<int>();
        }
        
        private RolePermission ReadRolePermissionFromSql(SqlDataReader reader)
        {
            var obj = new RolePermission
            {
                Id = reader["Id"] != DBNull.Value ? (Guid)reader["Id"] : Guid.Empty,
            };

            obj.PermissionSet = new PermissionSetDO()
            {
                Id = reader["PermissionSetId"] != DBNull.Value ? (Guid)reader["PermissionSetId"] : Guid.Empty,
                ObjectType = reader["ObjectType"] != DBNull.Value ? (string)reader["ObjectType"] : string.Empty,
            };

            var objRoleId = reader["roleId"] != DBNull.Value ? (string)reader["roleId"] : string.Empty;
            var objRoleName = reader["roleName"] != DBNull.Value ? (string)reader["roleName"] : string.Empty;

            obj.Role = new RoleDO()
            {
                RoleId = objRoleId,
                RoleName = objRoleName
            };

            return obj;
        }

        private void ReadObjectRolePermissionFromSql(SqlDataReader reader, ObjectRolePermissionsWrapper objectRolePermissionsWrapper)
        {
            var obj = ReadRolePermissionFromSql(reader);

            objectRolePermissionsWrapper.ObjectId = reader["ObjectId"] != DBNull.Value ? (string) reader["ObjectId"] : string.Empty;
            objectRolePermissionsWrapper.Type = reader["Type"] != DBNull.Value ? (string)reader["Type"] : string.Empty;
            //read property name and check for values
            var propertyName = reader["PropertyName"] != DBNull.Value ? (string) reader["PropertyName"] : string.Empty;

            if (string.IsNullOrEmpty(propertyName))
            {
                objectRolePermissionsWrapper.RolePermissions.Add(obj);
            }
            else
            {
                //check if the same property is already added to this list
                if (objectRolePermissionsWrapper.Properties.ContainsKey(propertyName))
                {
                    objectRolePermissionsWrapper.Properties[propertyName].Add(obj);
                }
                else
                {
                    objectRolePermissionsWrapper.Properties[propertyName] = new List<RolePermission> {obj};
                }
            }
        }
    }
}
