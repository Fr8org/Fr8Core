namespace Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MoveFr8TerminalsTxtToDB : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql(@"
MERGE TerminalRegistration AS T
USING(select 'localhost:51234' as Endpoint
        union select 'localhost:46281' as Endpoint
        union select 'localhost:50705' as Endpoint
        union select 'localhost:53234' as Endpoint
        union select 'localhost:39504' as Endpoint
        union select 'localhost:30699' as Endpoint
        union select 'localhost:47011' as Endpoint
        union select 'localhost:10601' as Endpoint
        union select 'localhost:25923' as Endpoint
        union select 'localhost:19760' as Endpoint
        union select 'localhost:30701' as Endpoint
        union select 'localhost:48317' as Endpoint
        union select 'localhost:47011' as Endpoint
        union select 'localhost:39768' as Endpoint
        union select 'localhost:39555' as Endpoint
        union select 'localhost:54642' as Endpoint
        union select 'localhost:22555' as Endpoint
        union select 'localhost:59022' as Endpoint) AS S
ON(T.Endpoint = S.Endpoint)
WHEN NOT MATCHED BY TARGET
    THEN INSERT(Endpoint, LastUpdated, CreateDate) VALUES(S.Endpoint, GetDate(), GetDate());");
        }

        public override void Down()
        {
        }
    }
}
