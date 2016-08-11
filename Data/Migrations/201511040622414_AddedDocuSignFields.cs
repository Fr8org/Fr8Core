namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDocuSignFields : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.DocuSignEvents");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DocuSignEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalEventType = c.Int(nullable: false),
                        RecipientId = c.String(),
                        EnvelopeId = c.String(),
                        DocuSignObject = c.String(),
                        Status = c.String(),
                        CreateDate = c.String(),
                        SentDate = c.String(),
                        DeliveredDate = c.String(),
                        CompletedDate = c.String(),
                        Email = c.String(),
                        EventId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
