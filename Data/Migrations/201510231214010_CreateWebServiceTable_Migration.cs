namespace Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	
	public partial class CreateWebServiceTable_Migration : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"dbo.WebServices",
				x => new
				{
					Id = x.Int(nullable: false, identity: true),
					Name = x.String(nullable: false),
					Icon = x.String(nullable: false),
					LastUpdated = x.DateTimeOffset(nullable: false, precision: 7),
					CreateDate = x.DateTimeOffset(nullable: false, precision: 7)
				})
				.PrimaryKey(x => x.Id);


		}
		
		public override void Down()
		{

			DropTable("dbo.WebServices");
		}
	}
}
