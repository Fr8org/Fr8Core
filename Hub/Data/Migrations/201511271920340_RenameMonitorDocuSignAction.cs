namespace Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	
	public partial class RenameMonitorDocuSignAction : System.Data.Entity.Migrations.DbMigration
	{
		public override void Up()
		{
			Sql("UPDATE ActivityTemplate SET Name = 'Monitor_DocuSign_Envelope_Activity', Label = 'Monitor DocuSign Envelope Activity' WHERE Name = 'Monitor_DocuSign'");
		}
		
		public override void Down()
		{
			Sql("UPDATE ActivityTemplate SET Name = 'Monitor_DocuSign', Label = 'Monitor DocuSign' WHERE Name = 'Monitor_DocuSign_Envelope_Activity'");
		}
	}
}
