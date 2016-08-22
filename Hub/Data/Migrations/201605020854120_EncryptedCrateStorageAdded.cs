namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class EncryptedCrateStorageAdded : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "EncryptedCrateStorage", c => c.Binary());

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
            DropColumn("dbo.Actions", "EncryptedCrateStorage");
            DropIndex("dbo.EncryptionTokens", "PeerId");
            DropTable("dbo.EncryptionTokens");
        }
    }
}
