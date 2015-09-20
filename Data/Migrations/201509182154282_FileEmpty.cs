namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FileEmpty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "DockyardAccountID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "DockyardAccountID");
        }
    }
}
