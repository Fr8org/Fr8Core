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
        private const string InsertRolePrivilegeCommand = "insert into dbo.RolePrivileges(id, privilegeName, roleId, createDate, lastUpdated) values (@id, @privilegeName, @roleId, @createDate, @lastUpdated)";
        private const string InsertObjectRolePrivilegeCommand = "insert into dbo.ObjectRolePrivileges(objectId, rolePrivilegeId, type, propertyName, createDate, lastUpdated) values (@objectId, @rolePrivilegeId, @type, @propertyName, @createDate, @lastUpdated)";
        
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

        public int InsertRolePrivilege(RolePrivilege rolePrivilege)
        {
            var affectedRows = Upsert(_sqlConnectionProvider, rolePrivilege, false, true);

            if (affectedRows == 0)
            {
                throw new Exception("Violation of unique constraint");
            }

            return affectedRows;
        }

        public int UpdateRolePrivilege(RolePrivilege rolePrivilege)
        {
            return Upsert(_sqlConnectionProvider, rolePrivilege, true, false);
        }

        private int Upsert(ISqlConnectionProvider sqlConnectionProvider, RolePrivilege rolePrivilege, bool allowUpdate, bool allowInsert)
        {
            using (var connection = OpenConnection(sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    if (rolePrivilege.Id == Guid.Empty)
                    {
                        rolePrivilege.Id = Guid.NewGuid();
                        command.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                    }
                    
                    command.Parameters.AddWithValue("@id", rolePrivilege.Id);
                    command.Parameters.AddWithValue("@privilegeName", rolePrivilege.Privilege.Name);
                    command.Parameters.AddWithValue("@roleId", rolePrivilege.Role.RoleId);
                    command.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                    var cmdText = String.Empty;
                    if (allowInsert)
                    {
                        cmdText = InsertRolePrivilegeCommand;
                    }

                    if (allowUpdate)
                    {
                        cmdText = "update dbo.RolePrivileges set privilegeName = @privilegeName, roleId = @roleId, lastUpdated = @lastUpdated";
                    }

                    command.CommandText = cmdText;
                    var affectedRows = command.ExecuteNonQuery();

                    return affectedRows;
                }
            }
        }

        public ObjectRolePrivilegesDO GetRolePrivilegesForSecuredObject(string dataObjectId)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd = "select rp.Id, rp.privilegeName, orp.PropertyName, orp.ObjectId as ObjectId, orp.Type, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate " +
                                 "from dbo.RolePrivileges rp                                                                            " +
                                 "inner join dbo.ObjectRolePrivileges orp on rp.Id = orp.RolePrivilegeId                                " +
                                 "inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id                                                  " +
                                 "where orp.ObjectId = @objectId";

                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.CommandText = cmd;

                    var result = new ObjectRolePrivilegesDO();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReadObjectRolePrivilegeFromSql(reader, result);
                        }
                    }

                    return result;
                }
            }
        }

        public List<RolePrivilege> GetRolePrivilegesForFr8Account(Guid fr8AccountId)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd = "select rp.id, rp.privilegeName, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate " +
                                 "from dbo.RolePrivileges rp                                                                            " +
                                 "inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id                                                  " +
                                 "inner join dbo.AspNetUserRoles anur on anr.Id = anur.RoleId                                           " +
                                 "where anur.UserId = @fr8AccountId";

                    command.Parameters.AddWithValue("@fr8AccountId", fr8AccountId);
                    command.CommandText = cmd;

                    var result = new List<RolePrivilege>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(ReadRolePrivilegeFromSql(reader));
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
                    var rolePrivilegeIds = new List<Guid>();

                    using (var selectCommand = new SqlCommand())
                    {
                        selectCommand.Connection = connection;
                        selectCommand.Transaction = transaction;

                        //select all rolePrivileges for roleName OwnerOfCurrentObject
                        selectCommand.Parameters.AddWithValue("@roleName", Roles.OwnerOfCurrentObject);
                        selectCommand.CommandText = "select rp.Id from dbo.RolePrivileges rp inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id where anr.Name = @roleName";

                        using (var reader = selectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rolePrivilegeIds.Add((Guid)reader["Id"]);
                            }
                        }
                    }

                    using (var insertCommand = new SqlCommand())
                    {
                        insertCommand.Connection = connection;
                        insertCommand.Transaction = transaction;

                        foreach (var rolePrivilegeId in rolePrivilegeIds)
                        {
                            insertCommand.Parameters.Clear();
                            insertCommand.Parameters.AddWithValue("@objectId", dataObjectId);
                            insertCommand.Parameters.AddWithValue("@rolePrivilegeId", rolePrivilegeId);
                            insertCommand.Parameters.AddWithValue("@type", dataObjectType);
                            insertCommand.Parameters.AddWithValue("@propertyName", DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                            insertCommand.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                            var cmdText = InsertObjectRolePrivilegeCommand;

                            insertCommand.CommandText = cmdText;
                            var affectedRows = insertCommand.ExecuteNonQuery();

                            if (affectedRows == 0)
                            {
                                throw new Exception("Problem with Inserting new ObjectRolePrivilege");
                            }
                        }
                    }
                    
                    transaction.Commit();
                }
            }
        }

        public int InsertObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string dataObjectType, string propertyName = null)
        {
            using (var connection = OpenConnection(_sqlConnectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    command.Parameters.AddWithValue("@objectId", dataObjectId);
                    command.Parameters.AddWithValue("@rolePrivilegeId", rolePrivilegeId);
                    command.Parameters.AddWithValue("@type", dataObjectType);
                    command.Parameters.AddWithValue("@propertyName", propertyName != null ? (object) propertyName : DBNull.Value);
                    command.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                    command.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                    var cmdText = InsertObjectRolePrivilegeCommand;
                    
                    command.CommandText = cmdText;
                    var affectedRows = command.ExecuteNonQuery();

                    if (affectedRows == 0)
                    {
                        throw new Exception("Problem with Inserting new ObjectRole Privilege");
                    }

                    return affectedRows;
                }
            }
        }

        public int RemoveObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string propertyName = null)
        {
            throw new NotImplementedException();
        }

        private RolePrivilege ReadRolePrivilegeFromSql(SqlDataReader reader)
        {
            var obj = new RolePrivilege
            {
                Id = reader["Id"] != DBNull.Value ? (Guid)reader["Id"] : Guid.Empty,
            };

            var privilege = reader["PrivilegeName"] != DBNull.Value ? (string) reader["PrivilegeName"] : string.Empty;

            obj.Privilege = new PrivilegeDO()
            {
                Name = privilege
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

        private void ReadObjectRolePrivilegeFromSql(SqlDataReader reader, ObjectRolePrivilegesDO objectRolePrivilegesDO)
        {
            var obj = ReadRolePrivilegeFromSql(reader);

            objectRolePrivilegesDO.ObjectId = reader["ObjectId"] != DBNull.Value ? (string) reader["ObjectId"] : string.Empty;
            objectRolePrivilegesDO.Type = reader["Type"] != DBNull.Value ? (string)reader["Type"] : string.Empty;
            //read property name and check for values
            var propertyName = reader["PropertyName"] != DBNull.Value ? (string) reader["PropertyName"] : string.Empty;

            if (string.IsNullOrEmpty(propertyName))
            {
                objectRolePrivilegesDO.RolePrivileges.Add(obj);
            }
            else
            {
                //check if the same property is already added to this list
                if (objectRolePrivilegesDO.Properties.ContainsKey(propertyName))
                {
                    objectRolePrivilegesDO.Properties[propertyName].Add(obj);
                }
                else
                {
                    objectRolePrivilegesDO.Properties[propertyName] = new List<RolePrivilege>() {obj};
                }
            }
        }
    }
}
