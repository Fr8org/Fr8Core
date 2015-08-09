namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIncidentStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Incidents", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Incidents", "Status");
        }
    }
}
