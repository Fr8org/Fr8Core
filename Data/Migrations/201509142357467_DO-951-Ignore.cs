namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DO951Ignore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plugins", "Endpoint", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plugins", "Endpoint");
            
        }
    }
}
