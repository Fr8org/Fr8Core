namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrganizationAndAccountToObjectRolePermissions : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ObjectRolePermissions", "Fr8AccountId", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.ObjectRolePermissions", "OrganizationId", c => c.Int(nullable: true));
        }

        public override void Down()
        {
            DropColumn("dbo.ObjectRolePermissions", "Fr8AccountId");
            DropColumn("dbo.ObjectRolePermissions", "OrganizationId");
        }
    }
}
