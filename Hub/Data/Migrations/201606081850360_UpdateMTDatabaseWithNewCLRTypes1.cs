namespace Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class UpdateMTDatabaseWithNewCLRTypes1 : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Fr8Data.Manifests', 'Fr8.Infrastructure.Data.Manifests')");
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Fr8Data.DataTransferObjects', 'Fr8.Infrastructure.Data.DataTransferObjects')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName, 'Fr8Data.Crates', 'Fr8.Infrastructure.Data.Crates')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName, ', Fr8Data,', ', Fr8Infrastructure.NET,')");
        }

        public override void Down()
        {
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Fr8.Infrastructure.Data.Manifests', 'Fr8Data.Manifests')");
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Fr8.Infrastructure.Data.DataTransferObjects', 'Fr8Data.DataTransferObjects')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName,  'Fr8.Infrastructure.Data.Crates', 'Fr8Data.Crates')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName,  ', Fr8Infrastructure.NET,', ', Fr8Data,')");
        }
    }
}
