namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddTextToAnswer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Answers", "Text", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Answers", "Text");
        }
    }
}
