namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActionTemplateDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActionTemplate", "ActionProcessor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActionTemplate", "ActionProcessor");
        }
    }
}
