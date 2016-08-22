namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FR3514Makeactivityclassesnamingconsistent : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE ActivityTemplate SET Name = 'Generate_Table_Activity'  WHERE Name LIKE 'GenerateTableActivity'");
            Sql("UPDATE ActivityTemplate SET Name = 'Save_To_File'  WHERE Name LIKE 'SaveToFile'");
            Sql("UPDATE ActivityTemplate SET Name = 'Set_Excel_Template'  WHERE Name LIKE 'SetExcelTemplate'");
            Sql("UPDATE ActivityTemplate SET Name = 'Add_Payload_Manually'  WHERE Name LIKE 'AddPayloadManually'");
            Sql("UPDATE ActivityTemplate SET Name = 'App_Builder'  WHERE Name LIKE 'AppBuilder'");
            Sql("UPDATE ActivityTemplate SET Name = 'Build_Query'  WHERE Name LIKE 'BuildQuery'");
            Sql("UPDATE ActivityTemplate SET Name = 'Connect_To_Sql'  WHERE Name LIKE 'ConnectToSql'");
            Sql("UPDATE ActivityTemplate SET Name = 'Convert_Crates'  WHERE Name LIKE 'ConvertCrates'");
            Sql("UPDATE ActivityTemplate SET Name = 'Convert_Related_Fields_Into_Table'  WHERE Name LIKE 'ConvertRelatedFieldsIntoTable'");
            Sql("UPDATE ActivityTemplate SET Name = 'Execute_Sql'  WHERE Name LIKE 'ExecuteSql'");
            Sql("UPDATE ActivityTemplate SET Name = 'Extract_Table_Field'  WHERE Name LIKE 'ExtractTableField'");
            Sql("UPDATE ActivityTemplate SET Name = 'Filter_Object_List_By_Incoming_Message'  WHERE Name LIKE 'FilterObjectListByIncomingMessage'");
            Sql("UPDATE ActivityTemplate SET Name = 'Get_Data_From_Fr8_Warehouse'  WHERE Name LIKE 'GetDataFromFr8Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'Get_File_From_Fr8_Store'  WHERE Name LIKE 'GetFileFromFr8Store'");            
            Sql("UPDATE ActivityTemplate SET Name = 'Make_A_Decision'  WHERE Name LIKE 'MakeADecision'");
            Sql("UPDATE ActivityTemplate SET Name = 'Manage_Plan'  WHERE Name LIKE 'ManagePlan'");
            Sql("UPDATE ActivityTemplate SET Name = 'Query_Fr8_Warehouse'  WHERE Name LIKE 'QueryFr8Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'Save_To_Fr8_Warehouse'  WHERE Name LIKE 'SaveToFr8Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'Search_Fr8_Warehouse'  WHERE Name LIKE 'SearchFr8Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'Set_Delay'  WHERE Name LIKE 'SetDelay'");
            Sql("UPDATE ActivityTemplate SET Name = 'Store_File'  WHERE Name LIKE 'StoreFile'");
            Sql("UPDATE ActivityTemplate SET Name = 'Test_Incoming_Data'  WHERE Name LIKE 'TestIncomingData'");
            Sql("UPDATE ActivityTemplate SET Name = 'Send_Email_Via_SendGrid'  WHERE Name LIKE 'SendEmailViaSendGrid'");
        }
        
        public override void Down()
        {
            Sql("UPDATE ActivityTemplate SET Name = 'GenerateTableActivity'  WHERE Name LIKE 'Generate_Table_Activity'");
            Sql("UPDATE ActivityTemplate SET Name = 'SaveToFile'  WHERE Name LIKE 'Save_To_File'");
            Sql("UPDATE ActivityTemplate SET Name = 'SetExcelTemplate'  WHERE Name LIKE 'Set_Excel_Template'");
            Sql("UPDATE ActivityTemplate SET Name = 'AddPayloadManually'  WHERE Name LIKE 'Add_Payload_Manually'");
            Sql("UPDATE ActivityTemplate SET Name = 'AppBuilder'  WHERE Name LIKE 'App_Builder'");
            Sql("UPDATE ActivityTemplate SET Name = 'BuildQuery'  WHERE Name LIKE 'Build_Query'");
            Sql("UPDATE ActivityTemplate SET Name = 'ConnectToSql'  WHERE Name LIKE 'Connect_To_Sql'");
            Sql("UPDATE ActivityTemplate SET Name = 'ConvertCrates'  WHERE Name LIKE 'Convert_Crates'");
            Sql("UPDATE ActivityTemplate SET Name = 'ConvertRelatedFieldsIntoTable'  WHERE Name LIKE 'Convert_Related_Fields_Into_Table'");
            Sql("UPDATE ActivityTemplate SET Name = 'ExecuteSql'  WHERE Name LIKE 'Execute_Sql'");
            Sql("UPDATE ActivityTemplate SET Name = 'ExtractTableField'  WHERE Name LIKE 'Extract_Table_Field'");
            Sql("UPDATE ActivityTemplate SET Name = 'FilterObjectListByIncomingMessage'  WHERE Name LIKE 'Filter_Object_List_By_Incoming_Message'");
            Sql("UPDATE ActivityTemplate SET Name = 'GetDataFromFr8Warehouse'  WHERE Name LIKE 'Get_Data_From_Fr8_Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'GetFileFromFr8Store'  WHERE Name LIKE 'Get_File_From_Fr8_Store'");
            Sql("UPDATE ActivityTemplate SET Name = 'MakeADecision'  WHERE Name LIKE 'Make_A_Decision'");
            Sql("UPDATE ActivityTemplate SET Name = 'ManagePlan'  WHERE Name LIKE 'Manage_Plan'");
            Sql("UPDATE ActivityTemplate SET Name = 'QueryFr8Warehouse'  WHERE Name LIKE 'Query_Fr8_Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'SaveToFr8Warehouse'  WHERE Name LIKE 'Save_To_Fr8_Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'SearchFr8Warehouse'  WHERE Name LIKE 'Search_Fr8_Warehouse'");
            Sql("UPDATE ActivityTemplate SET Name = 'SetDelay'  WHERE Name LIKE 'Set_Delay'");
            Sql("UPDATE ActivityTemplate SET Name = 'StoreFile'  WHERE Name LIKE 'Store_File'");
            Sql("UPDATE ActivityTemplate SET Name = 'TestIncomingData'  WHERE Name LIKE 'Test_Incoming_Data'");
            Sql("UPDATE ActivityTemplate SET Name = 'SendEmailViaSendGrid'  WHERE Name LIKE 'Send_Email_Via_SendGrid'");
        }
    }
}
