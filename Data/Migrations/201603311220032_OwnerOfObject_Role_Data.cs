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
            var readRolePrivilegeId = Guid.NewGuid();
            Sql($"insert into dbo.RolePrivileges(Id, PrivilegeName, RoleId, LastUpdated, CreateDate) values ('{readRolePrivilegeId}','{Privilege.ReadObject.ToString()}','{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())");
            var editRolePrivilegeId = Guid.NewGuid();
            Sql($"insert into dbo.RolePrivileges(Id, PrivilegeName, RoleId, LastUpdated, CreateDate) values ('{editRolePrivilegeId}','{Privilege.EditObject.ToString()}','{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())");
            var deleteRolePrivilegeId = Guid.NewGuid();
            Sql($"insert into dbo.RolePrivileges(Id, PrivilegeName, RoleId, LastUpdated, CreateDate) values ('{deleteRolePrivilegeId}','{Privilege.DeleteObject.ToString()}','{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())");

            var manageInternalUsersPrivilegeId = Guid.NewGuid();
            Sql($"insert into dbo.RolePrivileges(Id, PrivilegeName, RoleId, LastUpdated, CreateDate) values ('{manageInternalUsersPrivilegeId}','{Privilege.ManageInternalUsers.ToString()}','{newRoleId}', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())");


            //
            //create default security for all existing PlanDO, ActivityDO and ContainerDO in the database
            //every existing object need to be associated with 3 default rolePrivileges for 
            //

            //default security for plans
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{readRolePrivilegeId}', 'PlanDO', '{DateTimeOffset.UtcNow}', '{DateTimeOffset.UtcNow}' from dbo.Plans");
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{editRolePrivilegeId}', 'PlanDO', '{DateTimeOffset.UtcNow}', '{DateTimeOffset.UtcNow}' from dbo.Plans");
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{deleteRolePrivilegeId}', 'PlanDO', '{DateTimeOffset.UtcNow}', '{DateTimeOffset.UtcNow}' from dbo.Plans");

            //default security for containers
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{readRolePrivilegeId}', 'ContainerDO', CreateDate, LastUpdated from dbo.Containers");
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{editRolePrivilegeId}', 'ContainerDO', CreateDate, LastUpdated from dbo.Containers");
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{deleteRolePrivilegeId}', 'ContainerDO', CreateDate, LastUpdated from dbo.Containers");

            //default security for activites
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{readRolePrivilegeId}', 'ActivityDO', '{DateTimeOffset.UtcNow}', '{DateTimeOffset.UtcNow}' from dbo.Actions");
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{editRolePrivilegeId}', 'ActivityDO', '{DateTimeOffset.UtcNow}', '{DateTimeOffset.UtcNow}' from dbo.Actions");
            Sql($"insert into dbo.ObjectRolePrivileges(ObjectId, RolePrivilegeId, Type, CreateDate, LastUpdated)  select Id, '{deleteRolePrivilegeId}', 'ActivityDO', '{DateTimeOffset.UtcNow}', '{DateTimeOffset.UtcNow}' from dbo.Actions");
        }


        public override void Down()
        {
        }
    }
}
