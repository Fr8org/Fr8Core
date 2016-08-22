namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Table_RolePrivilege : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.RolePrivileges",
                c => new
                {
                    Id = c.Guid(nullable: false, identity: true),
                    PrivilegeName = c.String(),
                    RoleId = c.String(maxLength:128),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Id);

            CreateIndex("dbo.RolePrivileges","Id");
            CreateIndex("dbo.RolePrivileges", "RoleId");
            AddForeignKey("dbo.RolePrivileges","RoleId", "dbo.AspNetRoles","Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.RolePrivileges", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.RolePrivileges", "RoleId");
            DropIndex("dbo.RolePrivileges", "Id");
            DropTable("dbo.RolePrivileges");
        }
    }
}
