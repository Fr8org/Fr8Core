namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameMailerToEnvelope : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Mailers", newName: "Envelopes");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Envelopes", newName: "Mailers");
        }
    }
}
