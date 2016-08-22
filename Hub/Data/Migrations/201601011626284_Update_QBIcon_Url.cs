namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Update_QBIcon_Url : DbMigration
    {
        private string oldValue = "/Content/icons/web_services/quick-books-icon-64x64.png";
        private string newValue = "/Content/icons/web_services/quickbooks-icon-64x64.png";
        public override void Up()
        {
            Sql(String.Format(@"UPDATE [ws] SET [ws].[IconPath] = {0}
                                                FROM [dbo].[WebServices] as [ws]
                                                WHERE [ws].[Name] = 'QuickBooks'",newValue));
        }
        public override void Down()
        {
            Sql(String.Format(@"UPDATE [ws] SET [ws].[IconPath] = {0}
                                                FROM [dbo].[WebServices] as [ws]
                                                WHERE [ws].[Name] = 'QuickBooks'", oldValue));
        }
    }
}
