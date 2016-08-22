namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Renamed_ReceiveGoogleForm_To_MonitorFormResponses : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE [dbo].[ActivityTemplate] SET [Name] = 'Monitor_Form_Responses', [Label] = 'Monitor Form Responses', [Category] = 1 WHERE [Name] = 'Receive_Google_Form'");
        }
        public override void Down()
        {
            Sql(@"UPDATE [dbo].[ActivityTemplate] SET [Name] = 'Receive_Google_Form', [Label] = 'Receive Google Form Response', [Category] = 2 WHERE [Name] = 'Monitor_Form_Responses'");
        }
    }
}
