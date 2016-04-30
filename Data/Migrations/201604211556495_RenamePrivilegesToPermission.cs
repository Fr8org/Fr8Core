namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamePrivilegesToPermission : DbMigration
    {
        public override void Up()
        {
            Sql("EXEC sp_rename 'dbo.ObjectRolePrivileges', 'ObjectRolePermissions'");
            Sql("EXEC sp_rename 'dbo.ObjectRolePermissions.RolePrivilegeId', 'RolePermissionId', 'COLUMN'");
            Sql("EXEC sp_rename 'dbo.RolePrivileges', 'RolePermissions'");
            Sql("EXEC sp_rename 'dbo.RolePermissions.PrivilegeName', 'PermissionName', 'COLUMN'");
        }

        public override void Down()
        {
            Sql("EXEC sp_rename 'dbo.ObjectRolePermissions.RolePermissionId', 'RolePrivilegeId', 'COLUMN'");
            Sql("EXEC sp_rename 'dbo.ObjectRolePermissions', 'ObjectRolePrivileges'");
            Sql("EXEC sp_rename 'dbo.RolePermissions.PermissionName', 'PrivilegeName', 'COLUMN'");
            Sql("EXEC sp_rename 'dbo.RolePermissions', 'RolePrivileges'");
        }
    }
}
