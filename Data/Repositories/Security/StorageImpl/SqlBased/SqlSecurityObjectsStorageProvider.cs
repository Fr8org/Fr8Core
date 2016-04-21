using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Data.Repositories.Security.Entities;
using Data.Repositories.SqlBased;
using Data.States;

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
        private const string InsertRolePermissionCommand = "insert into dbo.RolePermission(id, permissionName, roleId, createDate, lastUpdated) values (@id, @permissionName, @roleId, @createDate, @lastUpdated)";
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
                    command.Parameters.AddWithValue("@permissionName", rolePermission.Permission.Name);
                    command.Parameters.AddWithValue("@roleId", rolePermission.Role.RoleId);
                    command.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                    var cmdText = String.Empty;
                    if (allowInsert)
                    {
                        cmdText = InsertRolePermissionCommand;
                    }

                    if (allowUpdate)
                    {
                        cmdText = "update dbo.RolePermissions set permissionName = @permissionName, roleId = @roleId, lastUpdated = @lastUpdated";
                    }

                    command.CommandText = cmdText;
                    var affectedRows = command.ExecuteNonQuery();

                    return affectedRows;
                }
            }
        }

        public ObjectRolePermissionsDO GetRolePermissionsForSecuredObject(string dataObjectId)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd = "select rp.Id, rp.permissionName, orp.PropertyName, orp.ObjectId as ObjectId, orp.Type, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate " +
                                 "from dbo.RolePermissions rp                                                                            " +
                                 "inner join dbo.ObjectRolePermissions orp on rp.Id = orp.RolePermissionId                                " +
                                 "inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id                                                  " +
                                 "where orp.ObjectId = @objectId";

                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.CommandText = cmd;

                    var result = new ObjectRolePermissionsDO();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReadObjectRolePermissionFromSql(reader, result);
                        }
                    }

                    return result;
                }
            }
        }

        public List<RolePermission> GetRolePermissionsForFr8Account(Guid fr8AccountId)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd = "select rp.id, rp.permissionName, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate " +
                                 "from dbo.RolePermissions rp                                                                            " +
                                 "inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id                                                  " +
                                 "inner join dbo.AspNetUserRoles anur on anr.Id = anur.RoleId                                           " +
                                 "where anur.UserId = @fr8AccountId";

                    command.Parameters.AddWithValue("@fr8AccountId", fr8AccountId);
                    command.CommandText = cmd;

                    var result = new List<RolePermission>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(ReadRolePermissionFromSql(reader));
                        }
                    }

                    return result;
                }
            }
        }

        public void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var rolePermissionIds = new List<Guid>();

                    using (var selectCommand = new SqlCommand())
                    {
                        selectCommand.Connection = connection;
                        selectCommand.Transaction = transaction;

                        //select all role permission for roleName OwnerOfCurrentObject
                        selectCommand.Parameters.AddWithValue("@roleName", Roles.OwnerOfCurrentObject);
                        selectCommand.CommandText = "select rp.Id from dbo.RolePermissions rp inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id where anr.Name = @roleName";

                        using (var reader = selectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rolePermissionIds.Add((Guid)reader["Id"]);
                            }
                        }
                    }

                    using (var insertCommand = new SqlCommand())
                    {
                        insertCommand.Connection = connection;
                        insertCommand.Transaction = transaction;

                        foreach (var rolePermissionId in rolePermissionIds)
                        {
                            insertCommand.Parameters.Clear();
                            insertCommand.Parameters.AddWithValue("@objectId", dataObjectId);
                            insertCommand.Parameters.AddWithValue("@rolePermissionIds", rolePermissionId);
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

        private RolePermission ReadRolePermissionFromSql(SqlDataReader reader)
        {
            var obj = new RolePermission
            {
                Id = reader["Id"] != DBNull.Value ? (Guid)reader["Id"] : Guid.Empty,
            };

            var permission = reader["PermissionName"] != DBNull.Value ? (string) reader["PermissionName"] : string.Empty;

            obj.Permission = new PermissionDO()
            {
                Name = permission
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

        private void ReadObjectRolePermissionFromSql(SqlDataReader reader, ObjectRolePermissionsDO objectRolePermissionDO)
        {
            var obj = ReadRolePermissionFromSql(reader);

            objectRolePermissionDO.ObjectId = reader["ObjectId"] != DBNull.Value ? (string) reader["ObjectId"] : string.Empty;
            objectRolePermissionDO.Type = reader["Type"] != DBNull.Value ? (string)reader["Type"] : string.Empty;
            //read property name and check for values
            var propertyName = reader["PropertyName"] != DBNull.Value ? (string) reader["PropertyName"] : string.Empty;

            if (string.IsNullOrEmpty(propertyName))
            {
                objectRolePermissionDO.RolePermissions.Add(obj);
            }
            else
            {
                //check if the same property is already added to this list
                if (objectRolePermissionDO.Properties.ContainsKey(propertyName))
                {
                    objectRolePermissionDO.Properties[propertyName].Add(obj);
                }
                else
                {
                    objectRolePermissionDO.Properties[propertyName] = new List<RolePermission>() {obj};
                }
            }
        }
    }
}
