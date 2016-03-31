using System.Security.AccessControl;
using Data.States;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OwnerOfObject_Role_Data : DbMigration
    {
        public override void Up()
        {
            var newRoleId = Guid.NewGuid();
            Sql($"insert into dbo.AspnetRoles(Id, Name, LastUpdated, CreateDate, Discriminator) values ('{newRoleId}','OwnerOfCurrentObject', '{DateTimeOffset.UtcNow}', '{DateTimeOffset.UtcNow}', 'AspNetRolesDO')");

            //connect all existing users with the new role
            Sql($"insert into dbo.AspNetUserRoles select Id,'{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 'AspNetUserRolesDO' from AspNetUsers");

            //insert default role privileges for role OwnerOfCurrentObject
            Sql($"insert into dbo.RolePrivileges(Id, PrivilegeName, RoleId, LastUpdated, CreateDate) values ('{Guid.NewGuid()}','{Privileges.ReadObject}','{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())");
            Sql($"insert into dbo.RolePrivileges(Id, PrivilegeName, RoleId, LastUpdated, CreateDate) values ('{Guid.NewGuid()}','{Privileges.EditObject}','{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())");
            Sql($"insert into dbo.RolePrivileges(Id, PrivilegeName, RoleId, LastUpdated, CreateDate) values ('{Guid.NewGuid()}','{Privileges.DeleteObject}','{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())");
        }

        public override void Down()
        {
        }
    }
}
