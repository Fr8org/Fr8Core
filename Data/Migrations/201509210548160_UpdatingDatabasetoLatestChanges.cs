namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingDatabasetoLatestChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plugins", "Version", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plugins", "Version");
        }
    }
}
