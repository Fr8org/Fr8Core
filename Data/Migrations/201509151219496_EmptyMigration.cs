namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmptyMigration : DbMigration
    {
        public override void Up()
        {
<<<<<<< HEAD
            // TODO: commented out by yakov.gnusin, this code broke EF migration.
            // DropColumn("dbo.Actions", "FieldMappingSettings");
            // DropColumn("dbo.Actions", "PayloadMappings");
=======
            DropColumn("dbo.Actions", "FieldMappingSettings");
            DropColumn("dbo.Actions", "PayloadMappings");
>>>>>>> parent of 4a77074... DO-1017
        }
        
        public override void Down()
        {
<<<<<<< HEAD
            // TODO: commented out by yakov.gnusin, this code broke EF migration.
            // AddColumn("dbo.Actions", "PayloadMappings", c => c.String());
            // AddColumn("dbo.Actions", "FieldMappingSettings", c => c.String());
=======
            AddColumn("dbo.Actions", "PayloadMappings", c => c.String());
            AddColumn("dbo.Actions", "FieldMappingSettings", c => c.String());
>>>>>>> parent of 4a77074... DO-1017
        }
    }
}
