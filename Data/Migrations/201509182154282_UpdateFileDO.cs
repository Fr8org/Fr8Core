namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FileEmpty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "DockyardAccountID", c => c.String(nullable: false, maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "DockyardAccountID");
        }
    }
}
