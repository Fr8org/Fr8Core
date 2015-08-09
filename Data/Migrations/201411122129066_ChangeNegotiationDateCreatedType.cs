namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeNegotiationDateCreatedType : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Negotiations", "DateCreated");
            AddColumn("dbo.Negotiations", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Negotiations", "DateCreated");
            AddColumn("dbo.Negotiations", "DateCreated", c => c.DateTime(nullable: false));
        }
    }
}
