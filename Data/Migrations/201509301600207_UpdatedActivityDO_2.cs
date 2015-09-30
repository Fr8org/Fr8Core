namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedActivityDO_2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActivityDOes", "ActivityDO_Id", "dbo.ActivityDOes");
            DropIndex("dbo.ActivityDOes", new[] { "ActivityDO_Id" });
            DropColumn("dbo.ActivityDOes", "ActivityDO_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActivityDOes", "ActivityDO_Id", c => c.Int());
            CreateIndex("dbo.ActivityDOes", "ActivityDO_Id");
            AddForeignKey("dbo.ActivityDOes", "ActivityDO_Id", "dbo.ActivityDOes", "Id");
        }
    }
}
