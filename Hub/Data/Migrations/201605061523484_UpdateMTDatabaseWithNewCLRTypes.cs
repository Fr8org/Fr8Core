namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UpdateMTDatabaseWithNewCLRTypes : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Data.Interfaces.Manifests', 'Fr8Data.Manifests')");
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Data.Interfaces.DataTransferObjects', 'Fr8Data.DataTransferObjects')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName, 'Data.Crates', 'Fr8Data.Crates')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName, ', Data,', ', Fr8Data,')");
        }

        public override void Down()
        {
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Fr8Data.Manifests', 'Data.Interfaces.Manifests')");
            Sql("UPDATE MtTypes SET ClrName =  REPLACE(ClrName, 'Fr8Data.DataTransferObjects', 'Data.Interfaces.DataTransferObjects')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName,  'Fr8Data.Crates', 'Data.Crates')");
            Sql("UPDATE MtTypes SET ClrName = REPLACE(ClrName,  ', Fr8Data,', ', Data,')");
        }
    }
}
