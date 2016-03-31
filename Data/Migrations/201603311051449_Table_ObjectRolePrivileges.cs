namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Table_ObjectRolePrivileges : DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.ObjectRolePrivileges",
                c => new
                {
                    ObjectId = c.Guid(nullable: false),
                    RolePrivilegeId = c.Guid(nullable:false),
                    Type = c.String(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => new { t.ObjectId, t.RolePrivilegeId });

            CreateIndex("dbo.ObjectRolePrivileges", "ObjectId");
            CreateIndex("dbo.ObjectRolePrivileges", "RolePrivilegeId");
            AddForeignKey("dbo.ObjectRolePrivileges", "RolePrivilegeId", "dbo.RolePrivileges", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ObjectRolePrivileges", "RolePrivilegeId", "dbo.RolePrivileges");
            DropIndex("dbo.ObjectRolePrivileges", "RolePrivilegeId");
            DropIndex("dbo.ObjectRolePrivileges", "ObjectId");
            DropTable("dbo.ObjectRolePrivileges");
        }
    }
}
