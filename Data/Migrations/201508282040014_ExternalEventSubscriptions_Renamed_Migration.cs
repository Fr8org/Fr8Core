namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExternalEventSubscriptions_Renamed_Migration : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ExternalEventRegistrations", newName: "ExternalEventSubscriptions");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ExternalEventSubscriptions", newName: "ExternalEventRegistrations");
        }
    }
}
