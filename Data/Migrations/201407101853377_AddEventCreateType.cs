using Data.States;

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddEventCreateType : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Events", new[] { "BookingRequestID" });
            CreateTable(
                "dbo.EventCreateTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Events", "CreateTypeID", c => c.Int(nullable: false, defaultValue: EventCreateType.KwasantBR));

            Sql(@"SET IDENTITY_INSERT dbo.EventCreateTypes ON;");
            Sql(string.Format(@"INSERT INTO dbo.EventCreateTypes (ID,NAME) VALUES ({0},'KwasantBR')", EventCreateType.KwasantBR));
            Sql(@"SET IDENTITY_INSERT dbo.EventCreateTypes OFF;");

            AlterColumn("dbo.Events", "BookingRequestID", c => c.Int());
            CreateIndex("dbo.Events", "BookingRequestID");
            CreateIndex("dbo.Events", "CreateTypeID");
            AddForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes");
            DropIndex("dbo.Events", new[] { "CreateTypeID" });
            DropIndex("dbo.Events", new[] { "BookingRequestID" });
            Sql(@"DELETE dbo.Events WHERE BookingRequestID IS NULL");
            AlterColumn("dbo.Events", "BookingRequestID", c => c.Int(nullable: false));
            DropColumn("dbo.Events", "CreateTypeID");
            DropTable("dbo.EventCreateTypes");
            CreateIndex("dbo.Events", "BookingRequestID");
        }
    }
}
