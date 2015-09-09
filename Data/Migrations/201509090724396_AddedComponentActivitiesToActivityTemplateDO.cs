namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedComponentActivitiesToActivityTemplateDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActionTemplate", "ComponentActivities", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActionTemplate", "ComponentActivities");
        }
    }
}
