namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HistoryDateIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("History", "CreateDate");
        }
        
        public override void Down()
        {
            DropIndex("History", "CreateDate");
        }
    }
}
