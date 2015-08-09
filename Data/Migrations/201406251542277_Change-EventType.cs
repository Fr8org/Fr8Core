namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ChangeEventType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventStatuses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Events", "StateID", c => c.Int(nullable: false));

            Sql(@"SET IDENTITY_INSERT dbo.EventStatuses ON;");

            Sql(@"Insert into EventStatuses (ID,NAME) VALUES (1,'Booking')");
            Sql(@"Insert into EventStatuses (ID,NAME) VALUES (2,'ReadyForDispatch')");
            Sql(@"Insert into EventStatuses (ID,NAME) VALUES (3,'DispatchCompleted')");

            Sql(@"SET IDENTITY_INSERT dbo.EventStatuses OFF;");

            //Set all 'Unknowns' to 'booking' by default
            Sql(@"Update Events set StateID = 1");

            Sql(@"Update Events set StateID = 1 WHERE state = 'Booking'");
            Sql(@"Update Events set StateID = 2 WHERE state = 'ReadyForDispatch'");
            Sql(@"Update Events set StateID = 3 WHERE state = 'DispatchCompleted'");

            CreateIndex("dbo.Events", "StateID");
            AddForeignKey("dbo.Events", "StateID", "dbo.EventStatuses", "Id", cascadeDelete: true);

            DropColumn("dbo.Events", "State");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Events", "State", c => c.String());
            DropForeignKey("dbo.Events", "StateID", "dbo.EventStatuses");
            DropIndex("dbo.Events", new[] { "StateID" });
            DropColumn("dbo.Events", "StateID");
            DropTable("dbo.EventStatuses");
        }
    }
}
