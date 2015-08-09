namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddFromEmailAddressAsOneToOne : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Emails", "FromID", c => c.Int(nullable: false));

            Sql(@"update emails
set fromid = emailaddressid
from emails
inner join recipients on emails.id = recipients.emailid and type = 1
");
            CreateIndex("dbo.Emails", "FromID");
            AddForeignKey("dbo.Emails", "FromID", "dbo.EmailAddresses", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Emails", "FromID", "dbo.EmailAddresses");
            DropIndex("dbo.Emails", new[] { "FromID" });
            DropColumn("dbo.Emails", "FromID");
        }
    }
}
