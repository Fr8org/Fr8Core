namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsFr8OwnTerminal_Property : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TerminalRegistration", "IsFr8OwnTerminal", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TerminalRegistration", "IsFr8OwnTerminal");
        }
    }
}
