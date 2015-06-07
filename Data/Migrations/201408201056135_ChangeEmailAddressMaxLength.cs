namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeEmailAddressMaxLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.EmailAddresses", "Address", c => c.String(nullable: false, maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.EmailAddresses", "Address", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
