namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteOldLogs : DbMigration
    {
        public override void Up()
        {
            Sql("DELETE FROM History  WHERE CreateDate < DATEADD(day, -14, GETDATE())");
        }
        
        public override void Down()
        {
        }
    }
}
