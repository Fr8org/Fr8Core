namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sync_26112014 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Incidents", "Data", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Incidents", "Data");
        }
    }
}
