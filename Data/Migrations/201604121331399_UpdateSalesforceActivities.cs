namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSalesforceActivities : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE ActivityTemplate SET Tags = 'Table Data Generator' WHERE [Name] = 'Get_Data'");
            Sql(@"INSERT INTO ActivityTemplate (
                    Name, 
                    Version, 
                    TerminalId, 
                    Category, 
                    LastUpdated,
                    CreateDate,
                    Label, 
                    MinPaneWidth, 
                    Tags, 
                    WebServiceId, 
                    ActivityTemplateState, 
                    Description, 
                    Type, 
                    NeedsAuthentication, 
                    ClientVisibility) 
                VALUES (
                    'MailMergeFromSalesforce', 
                    1, 
                    3, 
                    5, 
                    '00010101',
                    '00010101',
                    'Mail Merge from Salesforce', 
                    500, 
                    'UsesReconfigureList', 
                    7, 
                    1, 
                    'This solution extracts data from Salesforce and sends emails based on this data', 
                    1, 
                    1, 
                    1)");
            Sql(@"UPDATE ActivityTemplate SET Tags = 'AggressiveReload,Email Deliverer' WHERE [Name] = 'Send_DocuSign_Envelope'");
            Sql(@"UPDATE ActivityTemplate SET Tags = 'Notifier,Email Deliverer' WHERE [Name] = 'SendEmailViaSendGrid'");
        }
        
        public override void Down()
        {
            Sql(@"UPDATE ActivityTemplate SET Tags = NULL WHERE [Name] = 'Get_Data'");
            Sql(@"DELETE FROM ActivityTemplate WHERE [Name] = 'MailMergeFromSalesforce'");
            Sql(@"UPDATE ActivityTemplate SET Tags = 'AggressiveReload' WHERE [Name] = 'Send_DocuSign_Envelope'");
            Sql(@"UPDATE ActivityTemplate SET Tags = 'Notifier' WHERE [Name] = 'SendEmailViaSendGrid'");
        }
    }
}
