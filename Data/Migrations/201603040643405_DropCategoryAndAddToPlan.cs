namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropCategoryAndAddToPlan : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Plans", "Category");
            AddColumn("dbo.Plans", "Category", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plans", "Category");
        }
    }
}
