namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateAspNetRolesDiscriminator : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE dbo.[AspNetRoles] SET Discriminator = 'AspNetRolesDO'");
        }
        
        public override void Down()
        {
            Sql(@"UPDATE dbo.[AspNetRoles] SET Discriminator = 'IdentityRole'");
        }
    }
}
