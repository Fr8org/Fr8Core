namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Envelope_Migration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Envelopes", "EmailID", "dbo.Emails");
            DropIndex("dbo.Envelopes", new[] { "EmailID" });
            AddColumn("dbo.Envelopes", "Status", c => c.String());
            AddColumn("dbo.Envelopes", "DocusignEnvelopeId", c => c.String());
            DropColumn("dbo.Envelopes", "Handler");
            DropColumn("dbo.Envelopes", "TemplateName");
            DropColumn("dbo.Envelopes", "TemplateDescription");
            DropColumn("dbo.Envelopes", "EmailID");
            DropColumn("dbo.Envelopes", "MergeData");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Envelopes", "MergeData", c => c.String());
            AddColumn("dbo.Envelopes", "EmailID", c => c.Int(nullable: false));
            AddColumn("dbo.Envelopes", "TemplateDescription", c => c.String());
            AddColumn("dbo.Envelopes", "TemplateName", c => c.String());
            AddColumn("dbo.Envelopes", "Handler", c => c.String());
            DropColumn("dbo.Envelopes", "DocusignEnvelopeId");
            DropColumn("dbo.Envelopes", "Status");
            CreateIndex("dbo.Envelopes", "EmailID");
            AddForeignKey("dbo.Envelopes", "EmailID", "dbo.Emails", "Id", cascadeDelete: true);
        }
    }
}
