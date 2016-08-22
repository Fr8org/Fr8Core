namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Label_Property_To_ActionTemplate : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "Label", c => c.String());
            AddColumn("dbo.ActivityTemplate", "Label", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "Label");
            DropColumn("dbo.Actions", "Label");
        }
    }
}
