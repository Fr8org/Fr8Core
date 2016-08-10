namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPlanTemplatesReferenceToPageDefinitions : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PageDefinitions", "PlanTemplatesIds", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PageDefinitions", "PlanTemplatesIds");
        }
    }
}
