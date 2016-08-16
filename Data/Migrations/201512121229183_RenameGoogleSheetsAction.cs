namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameGoogleSheetsAction : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE ActivityTemplate SET Name='Get_Google_Sheet_Data', Label='Get Google Sheet Data' WHERE Name='Extract_Spreadsheet_Data'");
        }
        
        public override void Down()
        {
            Sql("UPDATE ActivityTemplate SET Name='Extract_Spreadsheet_Data', Label='Extract Spreadsheet Data' WHERE Name='Get_Google_Sheet_Data'");
        }
    }
}
