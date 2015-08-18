namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixEnvelopeDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Envelopes", "EnvelopeStatus", c => c.Int(nullable: false));
            DropColumn("dbo.Envelopes", "Status");
            DropColumn("dbo.Envelopes", "LastUpdated");
            DropColumn("dbo.Envelopes", "CreateDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Envelopes", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Envelopes", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Envelopes", "Status", c => c.Int(nullable: false));
            DropColumn("dbo.Envelopes", "EnvelopeStatus");
        }
    }
}
