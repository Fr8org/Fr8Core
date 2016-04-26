namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EncryptedCrateStorageAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "EncryptedCrateStorage", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Actions", "EncryptedCrateStorage");
        }
    }
}
