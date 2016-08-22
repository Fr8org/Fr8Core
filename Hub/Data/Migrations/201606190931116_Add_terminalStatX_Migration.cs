namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_terminalStatX_Migration : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql(@"
                MERGE TerminalRegistration AS T
                    USING(select 'localhost:48675' as Endpoint) AS S
                    ON(T.Endpoint = S.Endpoint)
                    WHEN NOT MATCHED BY TARGET  
                THEN INSERT(Endpoint, LastUpdated, CreateDate) VALUES(S.Endpoint, GetDate(), GetDate());");
        }
        
        public override void Down()
        {
        }
    }
}
