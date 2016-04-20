namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MMSalesforceSolution : DbMigration
    {
        public override void Up()
        {
//            Sql($@"
//MERGE ActivityTemplate AS Target
//USING (SELECT 'MailMergeFromSalesforce' as Name) AS SOURCE
//ON (Target.Name = Source.Name)
//WHEN MATCHED THEN
//	UPDATE SET Target.ActivityTemplateState = 1, Target.TerminalId = (SELECT TOP 1 Id FROM Terminals WHERE Name = 'terminalSalesforce')		
//WHEN NOT MATCHED BY TARGET THEN
//	INSERT (
//        Id,
//		Name, 
//		Version, 
//		TerminalId, 
//		Category,
//		LastUpdated,
//		CreateDate,
//		Label,
//		MinPaneWidth,
//		Tags,
//		WebServiceId,
//		ActivityTemplateState,
//		Description,
//		Type,
//		NeedsAuthentication,
//		ClientVisibility)
//	VALUES (
//        '{Guid.NewGuid().ToString()}',
//		'MailMergeFromSalesforce', 
//		'1', 
//		(SELECT TOP 1 Id FROM Terminals WHERE Name = 'terminalSalesforce'),
//		5,
//		GETDATE(),
//		GETDATE(),
//		'Mail Merge from Salesforce',
//		550,
//		'UsesReconfigureList',
//		7,
//		1,
//		'Retrieves specified data from Salesforce and process this data using specified email sender',
//		1,
//		1,
//		1);");
//            Sql("UPDATE ActivityTemplate SET Tags = 'AggressiveReload,Email Deliverer' WHERE Name = 'Send_DocuSign_Envelope'");
//            Sql("UPDATE ActivityTemplate SET Tags = 'Notifier,Email Deliverer' WHERE Name = 'SendEmailViaSendGrid'");
        }

        public override void Down()
        {
            //Sql("DELETE FROM ActivityTemplate WHERE Name = 'MailMergeFromSalesforce'");
            //Sql("UPDATE ActivityTemplate SET Tags = 'AggressiveReload' WHERE Name = 'Send_DocuSign_Envelope'");
            //Sql("UPDATE ActivityTemplate SET Tags = 'Notifier' WHERE Name = 'SendEmailViaSendGrid'");
        }
    }
}
