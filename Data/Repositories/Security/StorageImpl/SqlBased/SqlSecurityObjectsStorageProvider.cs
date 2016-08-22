using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Data.Entities;
using Data.Repositories.Security.Entities;
using Data.Repositories.SqlBased;
using Data.States;
using Data.States.Templates;
using Fr8.Infrastructure.Data.DataTransferObjects;

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
        private const string InsertRolePermissionCommand = "insert into dbo.RolePermissions(id, permissionSetId, roleId, createDate, lastUpdated) values (@id, @permissionSetId, (select top 1 Id from dbo.AspNetRoles where Name = @roleName), @createDate, @lastUpdated)";
        private const string InsertObjectRolePermissionCommand = "insert into dbo.ObjectRolePermissions(objectId, rolePermissionId, type, propertyName, fr8AccountId, organizationId, createDate, lastUpdated) values (@objectId, @rolePermissionId, @type, @propertyName, @fr8AccountId, @organizationId, @createDate, @lastUpdated)";
        
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

        public RolePermission GetRolePermission(string roleName, Guid permissionSetId)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd = @"select rp.Id, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate, p.Id as PermissionSetId, p.ObjectType
                                        from dbo.RolePermissions rp          
                                        inner join dbo.PermissionSets p on rp.PermissionSetId = p.Id                                                                  
                                        inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id
                                        where anr.[Name] = @roleName and p.Id = @permissionSetId";

                    command.Parameters.AddWithValue("@roleName", roleName);
                    command.Parameters.AddWithValue("@permissionSetId", permissionSetId);
                    command.CommandText = cmd;

                    RolePermission result = null;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = ReadRolePermissionFromSql(reader);
                        }
                    }
                    
                    return result;
                }
            }
        }

        public List<string> GetAllowedUserRolesForSecuredObject(Guid objectId, string objectType)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                var roles = new List<string>();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd = @" select distinct anr.Name from dbo.ObjectRolePermissions orp 
	                                        inner join dbo.RolePermissions rp on orp.RolePermissionId = rp.Id
	                                        inner join dbo.AspNetRoles  anr on rp.RoleId =anr.Id
                                          where orp.ObjectId = @objectId and orp.Type = @objectType   ";

                    command.Parameters.AddWithValue("@objectId", objectId);
                    command.Parameters.AddWithValue("@objectType", objectType);
                    command.CommandText = cmd;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["Name"] != DBNull.Value)
                            {
                               roles.Add((string)reader["Name"]);  
                            }   
                        }
                    }

                    return roles;
                }
            }
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
                    command.Parameters.AddWithValue("@roleName", rolePermission.Role.RoleName);
                    command.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                    var cmdText = String.Empty;
                    if (allowInsert)
                    {
                        cmdText = InsertRolePermissionCommand;
                    }

                    if (allowUpdate)
                    {
                        cmdText = "update dbo.RolePermissions set PermissionSetId = @permissionSetId, roleId = (select top 1 Id from dbo.AspNetRoles where Name = @roleName), lastUpdated = @lastUpdated";
                    }

                    command.CommandText = cmdText;
                    var affectedRows = command.ExecuteNonQuery();

                    return affectedRows;
                }
            }
        }

        public ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(Guid dataObjectId, string dataObjectType)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd =
                        @"select rp.Id, orp.PropertyName, orp.ObjectId as ObjectId, orp.Type, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate,
                            p.Id as PermissionSetId, p.ObjectType, orp.Fr8AccountId, orp.OrganizationId
                          from dbo.RolePermissions rp          
                          inner join dbo.PermissionSets p on rp.PermissionSetId = p.Id                                                                  
                          inner join dbo.ObjectRolePermissions orp on rp.Id = orp.RolePermissionId                               
                          inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id where orp.ObjectId = @objectId and orp.Type = @objectType ";

                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.Parameters.AddWithValue("@objectType", dataObjectType);
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
                        var selectPermissionSetSql = "select PermissionTypeTemplateId from dbo.PermissionSetPermissions where PermissionSetId = @permissionSetId";

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
                                    Id = reader["PermissionTypeTemplateId"] != DBNull.Value ? (int) reader["PermissionTypeTemplateId"] : 0
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

        public void SetDefaultRecordBasedSecurityForObject(string currentUserId, string roleName, Guid dataObjectId, string dataObjectType, Guid rolePermissionId, int? organizationId = null, List<PermissionType> customPermissionTypes = null)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    //check if already exist a record inserted and prevent duplication 
                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.Parameters.AddWithValue("@rolePermissionId", rolePermissionId);
                    command.Parameters.AddWithValue("@type", dataObjectType);
                    command.CommandText = "select count(*) as existingData from dbo.ObjectRolePermissions where objectId = @objectId and rolePermissionId = @rolePermissionId and type = @type";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var existingData = reader["existingData"] != DBNull.Value ? (int)reader["existingData"] : 0;
                            //permissions for this object for a given role were already applied. in that case don't insert any data inside ObjectRolePermissions
                            if (existingData > 0)
                                return;
                        }
                    }

                    command.Parameters.AddWithValue("@fr8AccountId", currentUserId);
                    command.Parameters.AddWithValue("@organizationId", (organizationId.HasValue) ? (object)organizationId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@propertyName", DBNull.Value);
                    command.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                    command.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                    var cmdText = InsertObjectRolePermissionCommand;

                    command.CommandText = cmdText;
                    var affectedRows = command.ExecuteNonQuery();

                    if (affectedRows == 0)
                    {
                        throw new Exception("Problem with Inserting new ObjectRolePermission");
                    }
                }
            }
        }

        public int InsertObjectRolePermission(string currentUserId, Guid dataObjectId, Guid rolePermissionId, string dataObjectType, string propertyName = null)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.Parameters.AddWithValue("@rolePermissionId", rolePermissionId);
                    command.Parameters.AddWithValue("@type", dataObjectType);
                    command.Parameters.AddWithValue("@currentUserId", currentUserId);
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

        public int RemoveObjectRolePermission(Guid dataObjectId, Guid rolePermissionId, string propertyName = null)
        {
            throw new NotImplementedException();
        }

        public List<int> GetObjectBasedPermissionSetForObject(Guid dataObjectId, string dataObjectType, Guid profileId)
        {
            return new List<int>();
        }
        
        private RolePermission ReadRolePermissionFromSql(SqlDataReader reader)
        {
            return new RolePermission
            {   
                Id = reader["Id"] != DBNull.Value ? (Guid) reader["Id"] : Guid.Empty,
                PermissionSet = new PermissionSetDO()
                {
                    Id = reader["PermissionSetId"] != DBNull.Value ? (Guid) reader["PermissionSetId"] : Guid.Empty,
                    ObjectType = reader["ObjectType"] != DBNull.Value ? (string) reader["ObjectType"] : string.Empty,
                },
                Role = new RoleDO()
                {
                    RoleId = reader["roleId"] != DBNull.Value ? (string)reader["roleId"] : string.Empty,
                    RoleName = reader["roleName"] != DBNull.Value ? (string)reader["roleName"] : string.Empty
                }
            };
        }

        private void ReadObjectRolePermissionFromSql(SqlDataReader reader, ObjectRolePermissionsWrapper objectRolePermissionsWrapper)
        {
            var obj = ReadRolePermissionFromSql(reader);

            objectRolePermissionsWrapper.ObjectId = reader["ObjectId"] != DBNull.Value ? (Guid) reader["ObjectId"] : Guid.Empty;
            objectRolePermissionsWrapper.Type = reader["Type"] != DBNull.Value ? (string)reader["Type"] : string.Empty;
            objectRolePermissionsWrapper.Fr8AccountId = reader["Fr8AccountId"] != DBNull.Value ? (string)reader["Fr8AccountId"] : string.Empty;
            objectRolePermissionsWrapper.OrganizationId = reader["OrganizationId"] != DBNull.Value ? (int?)reader["OrganizationId"] : null;

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
