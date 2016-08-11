namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveActivityTemplateDupesAndAddConstaints : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            //FR-2338
            
            //Removing dupes
            Sql(@"DELETE FROM ActivityTemplate WHERE 
                Id NOT IN (SELECT MIN(Id) FROM ActivityTemplate GROUP BY Name, Version);");

            //adding constaint
            Sql(
                @"alter table ActivityTemplate 
                    alter column Name nvarchar(200) null
                    alter table ActivityTemplate 
                    alter column Version nvarchar(200) null
                    alter table ActivityTemplate add constraint NameVersionConstraint UNIQUE NONCLUSTERED
                    (Name, Version);");
            
        }
        
        public override void Down()
        {

            Sql(
                @"alter table ActivityTemplate 
                    alter column Name nvarchar(MAX) null
                    alter table ActivityTemplate 
                    alter column Version nvarchar(MAX) null
                  	ALTER TABLE ActivityTemplate DROP constraint NameVersionConstraint;");
        }
    }
}
