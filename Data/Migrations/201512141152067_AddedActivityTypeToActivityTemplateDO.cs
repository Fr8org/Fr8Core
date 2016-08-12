using Fr8.Infrastructure.Data.States;

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddedActivityTypeToActivityTemplateDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "Type", c => c.Int(nullable: false));
            Sql("UPDATE ActivityTemplate SET Type=" + (int)ActivityType.Standard);
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "Type");
        }
    }
}
