namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixUserRoles : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE AspNetUserRoles SET Discriminator = 'AspNetUserRolesDO'");
        }
        
        public override void Down()
        {
            Sql("UPDATE AspNetUserRoles SET Discriminator = 'IdentityUserRole'");
        }
    }
}
