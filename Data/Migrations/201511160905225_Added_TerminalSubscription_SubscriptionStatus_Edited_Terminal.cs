namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_TerminalSubscription_SubscriptionStatus_Edited_Terminal : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TerminalSubscription",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TerminalId = c.Int(nullable: false),
                        SubscriptionState = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UserDO_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._SubscriptionStateTemplate", t => t.SubscriptionState, cascadeDelete: true)
                .ForeignKey("dbo.Terminals", t => t.TerminalId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserDO_Id)
                .Index(t => t.TerminalId)
                .Index(t => t.SubscriptionState)
                .Index(t => t.UserDO_Id);
            
            CreateTable(
                "dbo._SubscriptionStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Terminals", "SubscriptionRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.Terminals", "UserDO_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Terminals", "UserDO_Id");
            AddForeignKey("dbo.Terminals", "UserDO_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TerminalSubscription", "UserDO_Id", "dbo.Users");
            DropForeignKey("dbo.TerminalSubscription", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.TerminalSubscription", "SubscriptionState", "dbo._SubscriptionStateTemplate");
            DropForeignKey("dbo.Terminals", "UserDO_Id", "dbo.Users");
            DropIndex("dbo.TerminalSubscription", new[] { "UserDO_Id" });
            DropIndex("dbo.TerminalSubscription", new[] { "SubscriptionState" });
            DropIndex("dbo.TerminalSubscription", new[] { "TerminalId" });
            DropIndex("dbo.Terminals", new[] { "UserDO_Id" });
            DropColumn("dbo.Terminals", "UserDO_Id");
            DropColumn("dbo.Terminals", "SubscriptionRequired");
            DropTable("dbo._SubscriptionStateTemplate");
            DropTable("dbo.TerminalSubscription");
        }
    }
}
