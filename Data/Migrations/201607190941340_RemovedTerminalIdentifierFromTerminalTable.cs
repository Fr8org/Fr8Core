namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedTerminalIdentifierFromTerminalTable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Terminals", "PublicIdentifier");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Terminals", "PublicIdentifier", c => c.String());
        }
    }
}
