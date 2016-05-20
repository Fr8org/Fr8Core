namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrganizationToObjectRolePermissions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ObjectRolePermissions", "OrganizationId", c => c.Int(nullable: true));
            CreateIndex("dbo.ObjectRolePermissions", "OrganizationId");
            AddForeignKey("dbo.ObjectRolePermissions", "OrganizationId", "dbo.Organizations", "Id");

        }

        public override void Down()
        {
            DropForeignKey("dbo.ObjectRolePermissions", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.ObjectRolePermissions", "OrganizationId");
            DropColumn("dbo.ObjectRolePermissions", "OrganizationId");
        }
    }
}
