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
                "dbo.PermissionSets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        ObjectType = c.String(),
                        ProfileId = c.Guid(),
                        HasFullAccess = c.Boolean(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Profiles", t => t.ProfileId)
                .Index(t => t.ProfileId);
            
            CreateTable(
                "dbo._PermissionTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PermissionSetPermissions",
                c => new
                    {
                        PermissionSetId = c.Guid(nullable: false),
                        PermissionTypeTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PermissionSetId, t.PermissionTypeTemplateId })
                .ForeignKey("dbo.PermissionSets", t => t.PermissionSetId, cascadeDelete: true)
                .ForeignKey("dbo._PermissionTypeTemplate", t => t.PermissionTypeTemplateId, cascadeDelete: true)
                .Index(t => t.PermissionSetId)
                .Index(t => t.PermissionTypeTemplateId);
            
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
            AddColumn("dbo.RolePermissions", "PermissionSetId", c => c.Guid(nullable: false));
            CreateIndex("dbo.RolePermissions", "PermissionSetId");
            AddForeignKey("dbo.RolePermissions", "PermissionSetId", "PermissionSets", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RolePermissions", "PermissionSetId", "PermissionSets");
            DropIndex("dbo.RolePermissions", "PermissionSetId");
            DropColumn("dbo.RolePermissions", "PermissionSetId");
            AddColumn("dbo.RolePermissions", "PermissionName", c => c.String());

            DropForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles");
            DropForeignKey("dbo.PermissionSets", "ProfileId", "dbo.Profiles");
            DropForeignKey("dbo.PermissionSetPermissions", "PermissionTypeTemplateId", "dbo._PermissionTypeTemplate");
            DropForeignKey("dbo.PermissionSetPermissions", "PermissionSetId", "dbo.PermissionSets");
            DropIndex("dbo.PermissionSetPermissions", new[] { "PermissionTypeTemplateId" });
            DropIndex("dbo.PermissionSetPermissions", new[] { "PermissionSetId" });
            DropIndex("dbo.PermissionSets", new[] { "ProfileId" });
            DropIndex("dbo.AspNetRoles", new[] { "ProfileId" });
            DropColumn("dbo.Terminals", "Label");
            DropTable("dbo.PermissionSetPermissions");
            DropTable("dbo._PermissionTypeTemplate");
            DropTable("dbo.PermissionSets");
            DropTable("dbo.Profiles");
        }
    }
}
