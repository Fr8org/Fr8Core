namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameActionRegistrationDOToActionTemplateDO : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ActionRegistration", newName: "ActionTemplate");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ActionTemplate", newName: "ActionRegistration");
        }
    }
}
