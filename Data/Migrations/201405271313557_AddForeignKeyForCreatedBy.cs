namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddForeignKeyForCreatedBy : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Events", name: "CreatedBy_Id", newName: "CreatedByID");
            RenameIndex(table: "dbo.Events", name: "IX_CreatedBy_Id", newName: "IX_CreatedByID");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Events", name: "IX_CreatedByID", newName: "IX_CreatedBy_Id");
            RenameColumn(table: "dbo.Events", name: "CreatedByID", newName: "CreatedBy_Id");
        }
    }
}
