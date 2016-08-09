namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Plan_Visibility : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Routes", "Visibility", c => c.Int(nullable: true));
            Sql("update [dbo].[Routes] set [Visibility] = 1");

            AlterColumn("dbo.Routes", "Visibility", c => c.Int(nullable: false));
            Sql("update [dbo].[Routes] set [Visibility] = 2 where [Name] = 'MonitorAllDocuSignEvents'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Routes", "Visibility");
        }
    }
}
