namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveNodeTransitions : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SubPlans", "NodeTransitions");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SubPlans", "NodeTransitions", c => c.String());
        }
    }
}
