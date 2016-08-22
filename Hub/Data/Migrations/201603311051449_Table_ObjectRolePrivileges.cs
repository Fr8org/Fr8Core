namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Table_ObjectRolePrivileges : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.ObjectRolePrivileges",
                c => new
                {
                    Id = c.Guid(nullable:false, identity:true),
                    ObjectId = c.String(nullable: false, maxLength: 128),
                    RolePrivilegeId = c.Guid(nullable:false),
                    Type = c.String(),
                    PropertyName = c.String(nullable:true),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                }).PrimaryKey(x=> x.Id);

            CreateIndex("dbo.ObjectRolePrivileges", "Id");
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
