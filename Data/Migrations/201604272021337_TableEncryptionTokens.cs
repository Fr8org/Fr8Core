using System.Data.Entity.Migrations;

namespace Data.Migrations
{
    public partial class TableEncryptionTokens : DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.EncryptionTokens",
                c => new
                {
                    PeerId = c.String(false, 256),
                    KeyId = c.String(false, 256),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.PeerId);

            CreateIndex("dbo.EncryptionTokens", "PeerId");
        }

        public override void Down()
        {
            DropIndex("dbo.EncryptionTokens", "PeerId");
            DropTable("dbo.EncryptionTokens");
        }
    }
}
