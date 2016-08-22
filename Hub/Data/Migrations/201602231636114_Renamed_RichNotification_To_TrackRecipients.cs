namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Renamed_RichNotification_To_TrackRecipients : DbMigration
    {   
        public override void Up()
        {
            Sql	(@"UPDATE [dbo].[ActivityTemplate] SET Name= 'Track_DocuSign_Recipients', Label='Track DocuSign Recipients' WHERE Name = 'Rich_Document_Notifications';
                   UPDATE [dbo].[Actions] SET Label = 'Track DocuSign Recipients' 
		                        WHERE ActivityTemplateId in 
			                        (SELECT Id FROM [dbo].[ActivityTemplate] 
				                        WHERE Name = 'Track_DocuSign_Recipients');");
        }
        
        public override void Down()
        {
            Sql(@"UPDATE [dbo].[ActivityTemplate] SET Name='Rich_Document_Notifications', Label='Rich Document Notifications' WHERE Name = 'Track_DocuSign_Recipients';
                UPDATE [dbo].[Actions] SET Label = 'Rich Document Notifications' 
		                WHERE ActivityTemplateId in 
			                (SELECT Id FROM [dbo].[ActivityTemplate] 
				                WHERE Name = 'Rich_Document_Notifications');");
        }
    }
}
