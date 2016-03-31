using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Data.Repositories.Security.Entities;
using Data.States;

namespace Data.Repositories.Security.StorageImpl.SqlBased
{
    public class SqlSecurityObjectsStorage : ISecurityObjectsStorage
    {
        private readonly string _connectionString;
        private const string InsertRolePrivilegeCommand = "insert into dbo.RolePrivileges(id, privilegeName, roleId, createDate, lastUpdated) values (@id, @privilegeName, @roleId, @createDate, @lastUpdated)";
        private const string InsertObjectRolePrivilegeCommand = "insert into dbo.ObjectRolePrivileges(objectId, rolePrivilegeId, createDate, lastUpdated) values (@id, @privilegeName, @roleId, @createDate, @lastUpdated)";

        private SqlConnection OpenConnection(ISqlConnectionProvider connectionProvider)
        {
            var connection = new SqlConnection((string)connectionProvider.ConnectionInfo);

            connection.Open();
            return connection;
        }

        public int InsertRolePrivilege(ISqlConnectionProvider connectionProvider, RolePrivilege rolePrivilege)
        {
            var affectedRows = Upsert(connectionProvider, rolePrivilege, false, true);

            if (affectedRows == 0)
            {
                throw new Exception("Violation of unique constraint");
            }

            return affectedRows;
        }

        public int UpdateRolePrivilege(ISqlConnectionProvider connectionProvider, RolePrivilege rolePrivilege)
        {
            return Upsert(connectionProvider, rolePrivilege, true, false);
        }

        private int Upsert(ISqlConnectionProvider connectionProvider, RolePrivilege rolePrivilege, bool allowUpdate, bool allowInsert)
        {
            using (var connection = OpenConnection(connectionProvider))
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
                    command.Parameters.AddWithValue("@privilegeName", rolePrivilege.PrivilegeName);
                    command.Parameters.AddWithValue("@roleId", rolePrivilege.RoleId);
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

        public IEnumerable<RolePrivilege> GetRolePrivilegesForSecuredObject(ISqlConnectionProvider connectionProvider, Guid securedObjectId)
        {
            using (var connection = OpenConnection(connectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    const string cmd = "select rp.id, rp.privilegeName, anr.Id as roleId, anr.Name as roleName, rp.lastUpdated, rp.createDate " +
                                 "from dbo.RolePrivileges rp                                                                            " +
                                 "inner join dbo.ObjectRolePrivileges orp on rp.Id = orp.RolePrivilegeId                                " +
                                 "inner join dbo.AspNetRoles anr on rp.RoleId = anr.Id                                                  " +
                                 "where orp.ObjectId = @objectId";

                    command.Parameters.AddWithValue("@objectId", securedObjectId);
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

        public IEnumerable<RolePrivilege> GetRolePrivilegesForFr8Account(ISqlConnectionProvider connectionProvider, Guid fr8AccountId)
        {
            using (var connection = OpenConnection(connectionProvider))
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

        public void SetupDefaultSecurityForDataObject(ISqlConnectionProvider connectionProvider, Guid dataObjectId)
        {
            using (var connection = OpenConnection(connectionProvider))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var rolePrivilegeIds = new List<Guid>();
                    //select rolePrivileges for roleName OwnerOfCurrentObject
                    using (var selectCommand = new SqlCommand())
                    {
                        selectCommand.Connection = connection;

                        selectCommand.Parameters.AddWithValue("@roleName", Roles.OwnerOfCurrentObject);
                        selectCommand.CommandText = "select id from dbo.RolePrivileges rp inner join dbo.AspNetRoles on rp.RoleId = anr.Id where anr.Name = @roleName";

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

                        foreach (var rolePrivilegeId in rolePrivilegeIds)
                        {
                            insertCommand.Parameters.Clear();
                            insertCommand.Parameters.AddWithValue("@objectId", dataObjectId);
                            insertCommand.Parameters.AddWithValue("@rolePrivilegeId", rolePrivilegeId);
                            insertCommand.Parameters.AddWithValue("@createDate", DateTimeOffset.UtcNow);
                            insertCommand.Parameters.AddWithValue("@lastUpdated", DateTimeOffset.UtcNow);

                            var cmdText = InsertObjectRolePrivilegeCommand;

                            insertCommand.CommandText = cmdText;
                            var affectedRows = insertCommand.ExecuteNonQuery();

                            if (affectedRows == 0)
                            {
                                throw new Exception("Problem with Inserting new ObjectRole Privilege");
                            }
                        }
                    }
                    
                    transaction.Commit();
                }
            }
        }

        public int InsertObjectRolePrivilege(ISqlConnectionProvider connectionProvider, Guid objectId, Guid rolePrivilegeId)
        {
            using (var connection = OpenConnection(connectionProvider))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    command.Parameters.AddWithValue("@objectId", objectId);
                    command.Parameters.AddWithValue("@rolePrivilegeId", rolePrivilegeId);
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

        public int RemoveObjectRolePrivilege(ISqlConnectionProvider connectionProvider, Guid objectId, Guid rolePrivilegeId)
        {
            throw new NotImplementedException();
        }

        private RolePrivilege ReadRolePrivilegeFromSql(SqlDataReader reader)
        {
            var obj = new RolePrivilege();

            obj.Id = reader["Id"] != DBNull.Value ? (Guid)reader["Id"] : Guid.Empty;
            obj.PrivilegeName = reader["PrivilegeName"] != DBNull.Value ? (string)reader["PrivilegeName"] : string.Empty;
            obj.RoleId = reader["RoleId"] != DBNull.Value ? (string)reader["RoleID"] : string.Empty;
            obj.RoleName = reader["RoleId"] != DBNull.Value ? (string)reader["RoleID"] : string.Empty;

            return obj;
        }
    }
}
