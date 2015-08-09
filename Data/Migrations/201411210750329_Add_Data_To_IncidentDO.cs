namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Data_To_IncidentDO : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.Incidents", "Data", c => c.String());
        }
        
        public override void Down()
        {
            //DropColumn("dbo.Incidents", "Data");
        }
    }
}
