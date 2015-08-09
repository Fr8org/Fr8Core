namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class StatusToEmailStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Emails", "EmailStatus", c => c.Int(nullable: false));
            DropColumn("dbo.Emails", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Emails", "Status", c => c.Int(nullable: false));
            DropColumn("dbo.Emails", "EmailStatus");
        }
    }
}
