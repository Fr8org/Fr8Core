namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Rename_EmailEmailAddresses_to_Recipients : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.EmailEmailAddresses", newName: "Recipients");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Recipients", newName: "EmailEmailAddresses");
        }
    }
}
