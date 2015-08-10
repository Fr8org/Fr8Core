namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class StartTrackingThreadInformationOnEmails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Emails", "MessageID", c => c.String());
            AddColumn("dbo.Emails", "References", c => c.String());

            Sql(@"UPDATE dbo.Emails SET MessageID = ('<' + convert(varchar(max), newid()) + '@sant.com>')");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Emails", "References");
            DropColumn("dbo.Emails", "MessageID");
        }
    }
}
