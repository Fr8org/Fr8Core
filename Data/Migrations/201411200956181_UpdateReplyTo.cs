namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateReplyTo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Emails", "ReplyToID", "dbo.EmailAddresses");
            DropIndex("dbo.Emails", new[] { "ReplyToID" });
            AddColumn("dbo.Emails", "ReplyToName", c => c.String());
            AddColumn("dbo.Emails", "ReplyToAddress", c => c.String());
            DropColumn("dbo.Emails", "ReplyToID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Emails", "ReplyToID", c => c.Int());
            DropColumn("dbo.Emails", "ReplyToAddress");
            DropColumn("dbo.Emails", "ReplyToName");
            CreateIndex("dbo.Emails", "ReplyToID");
            AddForeignKey("dbo.Emails", "ReplyToID", "dbo.EmailAddresses", "Id");
        }
    }
}
