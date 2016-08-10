namespace Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	
	public partial class RenameCollectFormDataSolution : System.Data.Entity.Migrations.DbMigration
	{
		public override void Up()
		{
			Sql("UPDATE ActivityTemplate SET Name = 'Extract_Data_From_Envelopes', Label = 'Extract Data From Envelopes' WHERE Name = 'Collect_Form_Data_Solution'");
		}
		
		public override void Down()
		{
			Sql("UPDATE ActivityTemplate SET Name = 'Collect_Form_Data_Solution', Label = 'Collect Form Data Solution' WHERE Name = 'Extract_Data_From_Envelopes'");
		}
	}
}
