namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRecipientEmailId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocuSignEvents", "RecipientEmail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocuSignEvents", "RecipientEmail");
        }
    }
}
