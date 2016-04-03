namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plans", "TestColumn", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plans", "TestColumn");
        }
    }
}
