namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixup2 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ProcessDOes", newName: "Processes");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Processes", newName: "ProcessDOes");
        }
    }
}
