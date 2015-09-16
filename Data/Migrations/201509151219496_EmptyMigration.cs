namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmptyMigration : DbMigration
    {
        public override void Up()
        {
            //DropColumn("dbo.Actions", "FieldMappingSettings");
            //DropColumn("dbo.Actions", "PayloadMappings");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.Actions", "PayloadMappings", c => c.String());
            //AddColumn("dbo.Actions", "FieldMappingSettings", c => c.String());
        }
    }
}
