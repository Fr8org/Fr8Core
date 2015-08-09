namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class rename_Kacts_to_Facts : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Kacts", newName: "Facts");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Facts", newName: "Kacts");
        }
    }
}
