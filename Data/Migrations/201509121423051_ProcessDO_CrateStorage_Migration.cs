namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessDO_CrateStorage_Migration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Processes", "CrateStorage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Processes", "CrateStorage");
        }
    }
}
