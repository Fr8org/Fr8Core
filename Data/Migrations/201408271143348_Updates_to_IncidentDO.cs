namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updates_to_IncidentDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Incidents", "ObjectId", c => c.Int(nullable: false));
            AddColumn("dbo.Incidents", "CustomerId", c => c.String());
            AddColumn("dbo.Incidents", "BookerId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Incidents", "BookerId");
            DropColumn("dbo.Incidents", "CustomerId");
            DropColumn("dbo.Incidents", "ObjectId");
        }
    }
}
