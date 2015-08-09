namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sync_Fix : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.Emails", "ConversationId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            //DropColumn("dbo.Emails", "ConversationId");
        }
    }
}
