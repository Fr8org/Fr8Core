namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActivityTypeToActivityTemplateDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "Type");
        }
    }
}
