namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveNameFromActivity : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Actions", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "Name", c => c.String());
        }
    }
}
