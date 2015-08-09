namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GeneralizeCreateDateField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExpectedResponses", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            DropColumn("dbo.Incidents", "CreateTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Incidents", "CreateTime", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            DropColumn("dbo.ExpectedResponses", "CreateDate");
        }
    }
}
