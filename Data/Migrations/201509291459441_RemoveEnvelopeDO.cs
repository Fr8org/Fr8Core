namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveEnvelopeDO : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Envelopes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Envelopes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EnvelopeStatus = c.Int(nullable: false),
                        DocusignEnvelopeId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
        }
    }
}
