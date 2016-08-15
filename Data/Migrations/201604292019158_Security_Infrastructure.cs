namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Security_Infrastructure : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProfileNodes", "ParentNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.ProfileItems", "ProfileNodeID", "dbo.ProfileNodes");
            DropForeignKey("dbo.ProfileNodes", "ProfileID", "dbo.Profiles");
            DropForeignKey("dbo.Profiles", "DockyardAccountID", "dbo.Users");
            DropForeignKey("dbo.PermissionSets", "ProfileId", "dbo.Profiles");
            DropForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles");
            DropIndex("dbo.Profiles", new[] { "DockyardAccountID", "Id" });
            DropIndex("dbo.ProfileNodes", new[] { "ProfileID" });
            DropIndex("dbo.ProfileNodes", new[] { "ParentNodeID" });
            DropIndex("dbo.ProfileItems", new[] { "ProfileNodeID" });
            DropPrimaryKey("dbo.Profiles");

            DropTable("dbo.Profiles");

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
            AlterColumn("dbo.Profiles", "Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.AspNetRoles", "ProfileId");
            AddForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles", "Id");
            DropTable("dbo.ProfileNodes");
            DropTable("dbo.ProfileItems");
            DropTable("dbo.ProfileNodeAncestorsCTEView");
            DropTable("dbo.ProfileNodeDescendantsCTEView");

            //update RolePermission table
            //delete tables ObjectRolePermissions and RolePermissions
            DropForeignKey("dbo.ObjectRolePrivileges", "RolePermissionId", "dbo.RolePermissions");
            DropIndex("dbo.ObjectRolePrivileges", "RolePermissionId");
            DropIndex("dbo.ObjectRolePrivileges", "ObjectId");
            DropTable("dbo.ObjectRolePrivileges");

            DropForeignKey("dbo.RolePrivileges", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.RolePrivileges", "RoleId");
            DropIndex("dbo.RolePrivileges", "Id");
            DropTable("dbo.RolePrivileges");

            CreateTable("dbo.RolePermissions",
                c => new
                {
                    Id = c.Guid(nullable: false, identity: true),
                    PermissionSetId = c.Guid(nullable: false),
                    RoleId = c.String(maxLength: 128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id);

            CreateIndex("dbo.RolePermissions", "Id");
            CreateIndex("dbo.RolePermissions", "RoleId");
            AddForeignKey("dbo.RolePermissions", "RoleId", "dbo.AspNetRoles", "Id");
            CreateIndex("dbo.RolePermissions", "PermissionSetId");
            AddForeignKey("dbo.RolePermissions", "PermissionSetId", "PermissionSets", "Id");

            CreateTable("dbo.ObjectRolePermissions",
                c => new
                {
                    Id = c.Guid(nullable: false, identity: true),
                    ObjectId = c.String(nullable: false, maxLength: 128),
                    RolePermissionId = c.Guid(nullable: false),
                    Type = c.String(),
                    PropertyName = c.String(nullable: true),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                }).PrimaryKey(x => x.Id);

            CreateIndex("dbo.ObjectRolePermissions", "Id");
            CreateIndex("dbo.ObjectRolePermissions", "RolePermissionId");
            AddForeignKey("dbo.ObjectRolePermissions", "RolePermissionId", "dbo.RolePermissions", "Id");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ProfileNodeDescendantsCTEView",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileNodeID = c.Int(nullable: false),
                        ProfileParentNodeID = c.Int(),
                        AnchorNodeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProfileNodeAncestorsCTEView",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileNodeID = c.Int(nullable: false),
                        ProfileParentNodeID = c.Int(),
                        AnchorNodeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProfileItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileNodeID = c.Int(nullable: false),
                        Key = c.String(),
                        Value = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProfileNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProfileID = c.Int(nullable: false),
                        ParentNodeID = c.Int(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Profiles", "DockyardAccountID", c => c.String(maxLength: 128));
            DropForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles");
            DropForeignKey("dbo.PermissionSets", "ProfileId", "dbo.Profiles");
            DropForeignKey("dbo.PermissionSetPermissions", "PermissionTypeTemplateId", "dbo._PermissionTypeTemplate");
            DropForeignKey("dbo.PermissionSetPermissions", "PermissionSetId", "dbo.PermissionSets");
            DropIndex("dbo.PermissionSetPermissions", new[] { "PermissionTypeTemplateId" });
            DropIndex("dbo.PermissionSetPermissions", new[] { "PermissionSetId" });
            DropIndex("dbo.PermissionSets", new[] { "ProfileId" });
            DropIndex("dbo.AspNetRoles", new[] { "ProfileId" });
            DropPrimaryKey("dbo.Profiles");
            AlterColumn("dbo.Profiles", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.AspNetRoles", "ProfileId");
            DropTable("dbo.PermissionSetPermissions");
            DropTable("dbo._PermissionTypeTemplate");
            DropTable("dbo.PermissionSets");
            AddPrimaryKey("dbo.Profiles", "Id");
            CreateIndex("dbo.ProfileItems", "ProfileNodeID");
            CreateIndex("dbo.ProfileNodes", "ParentNodeID");
            CreateIndex("dbo.ProfileNodes", "ProfileID");
            CreateIndex("dbo.Profiles", "DockyardAccountID");
            AddForeignKey("dbo.AspNetRoles", "ProfileId", "dbo.Profiles", "Id");
            AddForeignKey("dbo.PermissionSets", "ProfileId", "dbo.Profiles", "Id");
            AddForeignKey("dbo.Profiles", "DockyardAccountID", "dbo.Users", "Id");
            AddForeignKey("dbo.ProfileNodes", "ProfileID", "dbo.Profiles", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProfileItems", "ProfileNodeID", "dbo.ProfileNodes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProfileNodes", "ParentNodeID", "dbo.ProfileNodes", "Id");
        }
    }
}
