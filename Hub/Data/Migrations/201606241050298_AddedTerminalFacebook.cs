namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTerminalFacebook : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql(@"
                MERGE TerminalRegistration AS T
                    USING(select 'localhost:22666' as Endpoint) AS S
                    ON(T.Endpoint = S.Endpoint)
                    WHEN NOT MATCHED BY TARGET  
                THEN INSERT(Endpoint, LastUpdated, CreateDate) VALUES(S.Endpoint, GetDate(), GetDate());");
        }
        
        public override void Down()
        {
        }
    }
}
