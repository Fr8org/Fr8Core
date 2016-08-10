namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HistoryDateIndex : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql(@"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.History') AND NAME ='IX_CreateDate')
begin

    CREATE NONCLUSTERED INDEX[IX_CreateDate] ON[dbo].[History]
    (
        [CreateDate] ASC
    )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
end");
        }
        
        public override void Down()
        {
            Sql("DROP INDEX [IX_CreateDate] ON [dbo].[History]");
        }
    }
}
