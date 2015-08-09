namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddEventDateCreated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "DateCreated");
        }
    }
}
