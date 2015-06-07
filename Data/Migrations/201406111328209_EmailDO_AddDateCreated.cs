namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class EmailDO_AddDateCreated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Emails", "DateCreated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Emails", "DateCreated");
        }
    }
}
