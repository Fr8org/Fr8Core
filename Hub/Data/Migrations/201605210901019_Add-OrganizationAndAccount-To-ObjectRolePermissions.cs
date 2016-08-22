namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrganizationAndAccountToObjectRolePermissions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ObjectRolePermissions", "Fr8AccountId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.ObjectRolePermissions", "Fr8AccountId");
            AddForeignKey("dbo.ObjectRolePermissions", "Fr8AccountId", "dbo.Users", "Id");
            AddColumn("dbo.ObjectRolePermissions", "OrganizationId", c => c.Int(nullable: true));
            CreateIndex("dbo.ObjectRolePermissions", "OrganizationId");
            AddForeignKey("dbo.ObjectRolePermissions", "OrganizationId", "dbo.Organizations", "Id");
            var sqlMigration = @"
                declare @permissionSetId uniqueidentifier;
                set @permissionSetId = newid();
                
                declare @counter int;
                set @counter = (select count (*) from dbo._PermissionTypeTemplate)
                if(@counter = 0) 
                begin 
	                insert into dbo._PermissionTypeTemplate(id) values(1)
	                insert into dbo._PermissionTypeTemplate(id) values(2)
	                insert into dbo._PermissionTypeTemplate(id) values(3)
	                insert into dbo._PermissionTypeTemplate(id) values(4)
	                insert into dbo._PermissionTypeTemplate(id) values(5)
                end

                insert into dbo.PermissionSets(id, Name, ObjectType, CreateDate, LastUpdated, HasFullAccess) values(@permissionSetId, 'PermissionForOwners', 'PlanNodeDO', '2016-05-12 09:16:34.7926131 +00:00','2016-05-12 09:16:34.7926131 +00:00', 0)
                insert into dbo.PermissionSetPermissions(permissionSetId, PermissionTypeTemplateId) values(@permissionSetId, 1)
                insert into dbo.PermissionSetPermissions(permissionSetId, PermissionTypeTemplateId) values(@permissionSetId, 2)
                insert into dbo.PermissionSetPermissions(permissionSetId, PermissionTypeTemplateId) values(@permissionSetId, 3)
                insert into dbo.PermissionSetPermissions(permissionSetId, PermissionTypeTemplateId) values(@permissionSetId, 4)
                insert into dbo.PermissionSetPermissions(permissionSetId, PermissionTypeTemplateId) values(@permissionSetId, 5)

                declare @rolePermissionSetId uniqueidentifier;
                set @rolePermissionSetId = newid();
                declare @roleId nvarchar(128)
                set @roleId = (select top 1 id from dbo.AspNetRoles where Name = 'OwnerOfCurrentObject')
                insert into dbo.RolePermissions(id, permissionSetId, roleId, CreateDate, LastUpdated) values (@rolePermissionSetId, @permissionSetId, @roleId, '2016-05-12 09:16:34.7926131 +00:00','2016-05-12 09:16:34.7926131 +00:00')

                insert into dbo.ObjectRolePermissions(id, objectId, RolePermissionId, Type, Fr8AccountId, CreateDate, LastUpdated) 
                select newid(),id,@rolePermissionSetId, 'PlanNodeDO',Fr8AccountId, '2016-05-12 09:16:34.7926131 +00:00','2016-05-12 09:16:34.7926131 +00:00'  from dbo.PlanNodes";

            Sql(sqlMigration);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ObjectRolePermissions", "Fr8AccountId", "dbo.Users");
            DropIndex("dbo.ObjectRolePermissions", "Fr8AccountId");
            DropColumn("dbo.ObjectRolePermissions", "Fr8AccountId");
            DropForeignKey("dbo.ObjectRolePermissions", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.ObjectRolePermissions", "OrganizationId");
            DropColumn("dbo.ObjectRolePermissions", "OrganizationId");
        }
    }
}
