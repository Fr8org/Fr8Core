namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSecretFieldToTerminalDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Terminals", "Secret", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Terminals", "Secret");
        }
    }
}
