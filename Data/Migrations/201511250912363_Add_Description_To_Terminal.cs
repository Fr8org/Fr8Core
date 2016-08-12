namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Description_To_Terminal : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Terminals", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Terminals", "Description");
        }
    }
}
