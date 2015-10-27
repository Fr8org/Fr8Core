namespace Data.Migrations
{
	using System.Data.Entity.Migrations;
	
	public partial class ActivityTemplate_Add_WebServiceReference_Migration : DbMigration
	{
		public override void Up()
		{
			AddColumn("dbo.ActivityTemplate", "WebServiceId", c => c.Int(nullable: false));
			Sql(SqlHelper.DropDefaultConstraint(table: "dbo.ActivityTemplate", column: "WebServiceId"));
			CreateIndex("dbo.ActivityTemplate", "WebServiceId");
			AddForeignKey("dbo.ActivityTemplate", "WebServiceId", "dbo.WebServices", "Id");
		}
		
		public override void Down()
		{
			DropForeignKey("dbo.ActivityTemplate", "WebServiceId", "dbo.WebServices");
			DropIndex("dbo.ActivityTemplate", new[] { "WebServiceId" });
			DropColumn("dbo.ActivityTemplate", "WebServiceId");
		}
	}
}