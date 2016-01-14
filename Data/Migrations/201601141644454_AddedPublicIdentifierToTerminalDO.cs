namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPublicIdentifierToTerminalDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Terminals", "PublicIdentifier", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Terminals", "PublicIdentifier");
        }
    }
}
