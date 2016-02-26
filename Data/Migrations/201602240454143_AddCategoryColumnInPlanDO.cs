namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryColumnInPlanDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Routes", "Category", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Routes", "Category");
        }
    }
}
