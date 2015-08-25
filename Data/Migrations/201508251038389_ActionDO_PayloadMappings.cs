namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionDO_PayloadMappings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "PayloadMappings", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Actions", "PayloadMappings");
        }
    }
}
