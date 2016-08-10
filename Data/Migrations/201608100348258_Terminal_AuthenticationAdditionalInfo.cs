namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Terminal_AuthenticationAdditionalInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Terminals", "AuthenticationAdditionalInfo", c => c.String(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Terminals", "AuthenticationAdditionalInfo");
        }
    }
}
