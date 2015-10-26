namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTagsToActivityTemplateDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "Tags", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "Tags");
        }
    }
}
