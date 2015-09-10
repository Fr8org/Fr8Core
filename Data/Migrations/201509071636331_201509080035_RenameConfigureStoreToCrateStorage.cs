namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201509080035_RenameConfigureStoreToCrateStorage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "CrateStorage", c => c.String());
            DropColumn("dbo.Actions", "ConfigurationStore");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "ConfigurationStore", c => c.String());
            DropColumn("dbo.Actions", "CrateStorage");
        }
    }
}
