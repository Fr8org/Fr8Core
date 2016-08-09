namespace Data.Migrations
{
    using States;
    using System;
    using System.Data.Entity.Migrations;

    public partial class TerminalRegistrationEnhancements : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TerminalRegistration");
            CreateTable(
                "dbo._OperationalStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ParticipationStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TerminalRegistration", "Id", c => c.Guid(nullable: false, defaultValueSql: "newid()"));
            AddColumn("dbo.TerminalRegistration", "DevUrl", c => c.String());
            AddColumn("dbo.TerminalRegistration", "ProdUrl", c => c.String());
            AddColumn("dbo.TerminalRegistration", "OperationalState", c => c.Int(nullable: true, defaultValue: OperationalState.Active));
            AddColumn("dbo.TerminalRegistration", "ParticipationState", c => c.Int(nullable: true, defaultValue: ParticipationState.Approved));

            AlterColumn("dbo.TerminalRegistration", "Endpoint", c => c.String());
            AddPrimaryKey("dbo.TerminalRegistration", "Id");
            CreateIndex("dbo.TerminalRegistration", "OperationalState");
            CreateIndex("dbo.TerminalRegistration", "ParticipationState");
            AddForeignKey("dbo.TerminalRegistration", "OperationalState", "dbo._OperationalStateTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TerminalRegistration", "ParticipationState", "dbo._ParticipationStateTemplate", "Id", cascadeDelete: true);
            Sql("INSERT INTO _OperationalStateTemplate(Id, Name) VALUES (1, 'Active')   INSERT INTO _ParticipationStateTemplate(Id, Name) VALUES (1, 'Approved')");
            Sql("Update TerminalRegistration SET OperationalState = 1, ParticipationState = 1");
        }

        public override void Down()
        {
            DropForeignKey("dbo.TerminalRegistration", "ParticipationState", "dbo._ParticipationStateTemplate");
            DropForeignKey("dbo.TerminalRegistration", "OperationalState", "dbo._OperationalStateTemplate");
            DropIndex("dbo.TerminalRegistration", new[] { "ParticipationState" });
            DropIndex("dbo.TerminalRegistration", new[] { "OperationalState" });
            DropPrimaryKey("dbo.TerminalRegistration");
            AlterColumn("dbo.TerminalRegistration", "Endpoint", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.TerminalRegistration", "ParticipationState");
            DropColumn("dbo.TerminalRegistration", "OperationalState");
            DropColumn("dbo.TerminalRegistration", "ProdUrl");
            DropColumn("dbo.TerminalRegistration", "DevUrl");
            DropColumn("dbo.TerminalRegistration", "Id");
            DropTable("dbo._ParticipationStateTemplate");
            DropTable("dbo._OperationalStateTemplate");
            AddPrimaryKey("dbo.TerminalRegistration", "Endpoint");
        }
    }
}
