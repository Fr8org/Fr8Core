namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionConfigStoreChange : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "ConfigurationStore", c => c.String());
            DropColumn("dbo.Actions", "ConfigurationSettings");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "ConfigurationSettings", c => c.String());
            DropColumn("dbo.Actions", "ConfigurationStore");
        }
    }
}
