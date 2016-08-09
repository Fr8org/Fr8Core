namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_DockyardAccount_To_Fr8Account_Migration : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Routes", name: "DockyardAccount_Id", newName: "Fr8Account_Id");
            RenameIndex(table: "dbo.Routes", name: "IX_DockyardAccount_Id", newName: "IX_Fr8Account_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Routes", name: "IX_Fr8Account_Id", newName: "IX_DockyardAccount_Id");
            RenameColumn(table: "dbo.Routes", name: "Fr8Account_Id", newName: "DockyardAccount_Id");
        }
    }
}
