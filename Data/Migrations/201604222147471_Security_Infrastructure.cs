namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Security_Infrastructure : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Permissions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(),
                        ReadObject = c.Boolean(nullable: false),
                        CreateObject = c.Boolean(nullable: false),
                        EditObject = c.Boolean(nullable: false),
                        DeleteObject = c.Boolean(nullable: false),
                        ViewAllObjects = c.Boolean(nullable: false),
                        ModifyAllObjects = c.Boolean(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProfilePermissions",
                c => new
                    {
                        ProfileDO_Id = c.Guid(nullable: false),
                        PermissionDO_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProfileDO_Id, t.PermissionDO_Id })
                .ForeignKey("dbo.Profiles", t => t.ProfileDO_Id, cascadeDelete: true)
                .ForeignKey("dbo.Permissions", t => t.PermissionDO_Id, cascadeDelete: true)
                .Index(t => t.ProfileDO_Id)
                .Index(t => t.PermissionDO_Id);
            
            AddColumn("dbo.AspNetRoles", "ProfileId", c => c.Guid());
            CreateIndex("dbo.AspNetRoles", "ProfileId");
            AddForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles", "Id");

            //update RolePermission table
            //delete data from ObjectRolePermissions 
            //delete data from RolePermissions
            var deleteSql = @" DELETE FROM dbo.ObjectRolePermissions  
                               DELETE FROM dbo.RolePermissions";
            Sql(deleteSql);

            DropColumn("dbo.RolePermissions", "PermissionName");
            AddColumn("dbo.RolePermissions", "PermissionId", c => c.Guid(nullable: false));
            CreateIndex("dbo.RolePermissions", "PermissionId");
            AddForeignKey("dbo.RolePermissions", "PermissionId", "Permissions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles");
            DropForeignKey("dbo.ProfilePermissions", "PermissionId", "dbo.Permissions");
            DropForeignKey("dbo.ProfilePermissions", "ProfileId", "dbo.Profiles");
            DropIndex("dbo.ProfilePermissions", new[] { "PermissionId" });
            DropIndex("dbo.ProfilePermissions", new[] { "ProfileId" });
            DropIndex("dbo.AspNetRoles", new[] { "ProfileId" });
            DropColumn("dbo.AspNetRoles", "ProfileId");
            DropTable("dbo.ProfilePermissions");
            DropTable("dbo.Permissions");
            DropTable("dbo.Profiles");

            DropForeignKey("dbo.RolePermissions", "PermissionId", "Permissions");
            DropIndex("dbo.RolePermissions", "PermissionId");
            DropColumn("dbo.RolePermissions", "PermissionId");
            AddColumn("dbo.RolePermissions", "PermissionName", c => c.String());
        }
    }
}
