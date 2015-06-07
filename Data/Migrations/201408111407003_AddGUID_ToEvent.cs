namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGUID_ToEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "ExternalGUID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "ExternalGUID");
        }
    }
}
