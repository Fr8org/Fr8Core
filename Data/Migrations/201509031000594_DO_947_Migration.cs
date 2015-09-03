namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DO_947_Migration : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo._EventStatusTemplate", newName: "_ExternalEventTypeTemplate");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo._ExternalEventTypeTemplate", newName: "_EventStatusTemplate");
        }
    }
}
