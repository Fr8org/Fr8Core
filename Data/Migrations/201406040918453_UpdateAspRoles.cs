namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAspRoles : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetRoles", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.AspNetRoles", "AspNetUserRolesDO_UserId", c => c.String(maxLength: 128));
            AddColumn("dbo.AspNetRoles", "AspNetUserRolesDO_RoleId", c => c.String(maxLength: 128));
            AddColumn("dbo.AspNetUserRoles", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" });
            AddForeignKey("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" }, "dbo.AspNetUserRoles", new[] { "UserId", "RoleId" });

            //Remove all existing roles and role assignments - the table is updated and will be repopulated with the seeding
            Sql(@"delete from AspNetUserRoles
delete from AspNetRoles");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" }, "dbo.AspNetUserRoles");
            DropIndex("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" });
            DropColumn("dbo.AspNetUserRoles", "Discriminator");
            DropColumn("dbo.AspNetRoles", "AspNetUserRolesDO_RoleId");
            DropColumn("dbo.AspNetRoles", "AspNetUserRolesDO_UserId");
            DropColumn("dbo.AspNetRoles", "Discriminator");
        }
    }
}
