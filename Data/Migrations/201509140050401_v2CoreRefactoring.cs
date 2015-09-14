namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2CoreRefactoring : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Processes", "CrateStorage", c => c.String());
            DropColumn("dbo.Processes", "EnvelopeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Processes", "EnvelopeId", c => c.String());
            DropColumn("dbo.Processes", "CrateStorage");
        }
    }
}
