namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPublicIdentifierAndSecretoToTerminalDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Terminals", "PublicIdentifier", c => c.String());
            AddColumn("dbo.Terminals", "Secret", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Terminals", "Secret");
            DropColumn("dbo.Terminals", "PublicIdentifier");
        }
    }
}
