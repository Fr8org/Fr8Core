namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActionTemplateDOPluginDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plugins", "BaseEndPoint", c => c.String());
            DropColumn("dbo.ActionTemplate", "DefaultEndPoint");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActionTemplate", "DefaultEndPoint", c => c.String());
            DropColumn("dbo.Plugins", "BaseEndPoint");
        }
    }
}
