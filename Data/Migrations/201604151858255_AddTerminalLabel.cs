namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTerminalLabel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Terminals", "Label", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Terminals", "Label");
        }
    }
}
