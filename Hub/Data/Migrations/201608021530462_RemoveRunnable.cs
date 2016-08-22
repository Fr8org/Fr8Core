namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRunnable : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PlanNodes", "Runnable");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlanNodes", "Runnable", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[PlanNodes] SET [Runnable] = 1");
            AlterColumn("dbo.PlanNodes", "Runnable", c => c.Boolean(nullable: false));
        }
    }
}
