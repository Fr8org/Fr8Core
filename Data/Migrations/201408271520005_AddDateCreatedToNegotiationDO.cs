namespace Data.Migrations
{    
    using System.Data.Entity.Migrations;
    
    public partial class AddDateCreatedToNegotiationDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Negotiations", "DateCreated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Negotiations", "DateCreated");
        }
    }
}
