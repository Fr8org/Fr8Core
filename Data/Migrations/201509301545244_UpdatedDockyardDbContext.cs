namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedDockyardDbContext : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityDOes", "ActivityDO_Id", c => c.Int());
            CreateIndex("dbo.ActivityDOes", "ActivityDO_Id");
            AddForeignKey("dbo.ActivityDOes", "ActivityDO_Id", "dbo.ActivityDOes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActivityDOes", "ActivityDO_Id", "dbo.ActivityDOes");
            DropIndex("dbo.ActivityDOes", new[] { "ActivityDO_Id" });
            DropColumn("dbo.ActivityDOes", "ActivityDO_Id");
        }
    }
}
