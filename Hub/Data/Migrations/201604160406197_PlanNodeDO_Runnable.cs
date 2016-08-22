namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlanNodeDO_Runnable : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlanNodes", "Runnable", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[PlanNodes] SET [Runnable] = 1");
            AlterColumn("dbo.PlanNodes", "Runnable", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.PlanNodes", "Runnable");
        }
    }
}
