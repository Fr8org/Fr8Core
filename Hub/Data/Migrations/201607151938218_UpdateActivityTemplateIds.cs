namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActivityTemplateIds : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE PROCEDURE updateActivity @Name nvarchar(100), @Id uniqueidentifier, @Version nvarchar(max) = '1'
                  AS
                  PRINT N'Updating Actions table'
                  UPDATE
                      activities
                  SET
                      activities.ActivityTemplateId = @Id
                  FROM
                      Actions AS activities
                      INNER JOIN ActivityTemplate AS activityTemplate
                          ON activities.ActivityTemplateId = activityTemplate.Id
                  WHERE
                      activityTemplate.Name = @Name AND activityTemplate.Version = @Version
                  ");
            Sql(@"CREATE PROCEDURE updateActivityDescription @Name nvarchar(100), @Id uniqueidentifier, @Version nvarchar(max) = '1'
                  AS
                  PRINT N'Updating ActivityDescription table'
                  UPDATE
                      activities
                  SET
                      activities.ActivityTemplateId = @Id
                  FROM
                      ActivityDescriptions AS activities
                      INNER JOIN ActivityTemplate AS activityTemplate
                          ON activities.ActivityTemplateId = activityTemplate.Id
                  WHERE
                      activityTemplate.Name = @Name AND activityTemplate.Version = @Version
                  ");
            Sql(@"CREATE PROCEDURE updateActivityCategorySet @Name nvarchar(100), @Id uniqueidentifier, @Version nvarchar(max) = '1'
                  AS
                  PRINT N'Updating ActivityCategorySet table'
                  UPDATE
                      activities
                  SET
                      activities.ActivityTemplateId = @Id
                  FROM
                      ActivityCategorySet AS activities
                      INNER JOIN ActivityTemplate AS activityTemplate
                          ON activities.ActivityTemplateId = activityTemplate.Id
                  WHERE
                      activityTemplate.Name = @Name AND activityTemplate.Version = @Version
                  ");
            Sql(@"CREATE PROCEDURE updateActivityTables @Name nvarchar(100), @Id uniqueidentifier, @Version nvarchar(max) = '1'
                  AS
                  PRINT N'Updating '+ @Name +N' references'
                  EXECUTE updateActivity            @Name = @Name , @Id = @Id, @Version = @Version
                  EXECUTE updateActivityDescription @Name = @Name , @Id = @Id, @Version = @Version
                  EXECUTE updateActivityCategorySet @Name = @Name , @Id = @Id, @Version = @Version
                  ");
            Sql(@"                 
                  IF NOT EXISTS (SELECT * FROM ActivityTemplate WHERE Id = 'f7619e79-112e-43aa-ba43-118c1ffc98f3')
                  BEGIN
                  PRINT N'Updating ActivityTemplateIds'
                  ALTER TABLE Actions NOCHECK CONSTRAINT [FK_dbo.Actions_dbo.ActivityTemplate_ActivityTemplateId]
                  
                  ALTER TABLE ActivityDescriptions NOCHECK CONSTRAINT [FK_dbo.ActivityDescriptions_dbo.ActivityTemplate_ActivityTemplateId]
                  
                  ALTER TABLE ActivityCategorySet NOCHECK CONSTRAINT [FK_dbo.ActivityCategorySet_dbo.ActivityTemplate_ActivityTemplateId]
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_Form_Responses', @Id = 'f7619e79-112e-43aa-ba43-118c1ffc98f3'
                  
                  EXECUTE updateActivityTables @Name = 'FilterUsingRunTimeData', @Id = '9913A4C4-18A7-4423-AACE-02487BA8C21B' 
                  EXECUTE updateActivityTables @Name = 'Store_File', @Id = '1C4F979D-BC1C-4A4A-B370-049DBACD3678'
                  EXECUTE updateActivityTables @Name = 'Archive_DocuSign_Template', @Id = 'CE88F864-24D1-4362-BD9E-066A40B028B6'  
                  EXECUTE updateActivityTables @Name = 'Get_DocuSign_Envelope', @Id = '0DE0F1FC-EBD3-48A6-9DF4-06F396E9F8C3'  
                  EXECUTE updateActivityTables @Name = 'Extract_Data', @Id = '36E485D9-FC8D-4C37-8593-226E5FD553D4'  
                  EXECUTE updateActivityTables @Name = 'PlanLauncher', @Id = '86A28016-9BDE-4E62-A62D-3D48BB994C2D'  
                  EXECUTE updateActivityTables @Name = 'Receive_DocuSign_Envelope', @Id = 'B0BDC517-1E5C-4AF8-BCA6-4916328F94E5'  
                  EXECUTE updateActivityTables @Name = 'Extract_Spreadsheet_Data', @Id = 'BC16B3E3-F832-4D8B-A72C-51A401F53CA2'
                  EXECUTE updateActivityTables @Name = 'CollectData', @Id = '6D7EE641-0916-44C5-8846-54104D9A837E'  
                  EXECUTE updateActivityTables @Name = 'Get_DocuSign_Template', @Id = '5E92E326-06E3-4C5B-A1F9-7542E8CD7C07'  
                  EXECUTE updateActivityTables @Name = 'Record_DocuSign_Events', @Id = '256DFEB9-4B98-424F-A006-7612684E3FE8'  
                  EXECUTE updateActivityTables @Name = 'ManageRoute', @Id = '747FD7FD-FB3B-4D5D-9348-7BDF08C32A92'    
                  EXECUTE updateActivityTables @Name = 'Process_Personal_Report', @Id = '5D925036-AC68-41D5-9646-9853F04B05CC'   
                  EXECUTE updateActivityTables @Name = 'MapFields', @Id = 'F0A61DD1-D1D0-4580-BEDE-9AF066F8E896'   
                  EXECUTE updateActivityTables @Name = 'Process_Personnel_Report', @Id = 'B41DD807-4375-410E-AF19-D2F5C508004F'   
                  EXECUTE updateActivityTables @Name = 'Convert_TableData_To_AccountingTransactions', @Id = '83313A48-85FD-4EF3-A377-DAA450705C69'   
                  EXECUTE updateActivityTables @Name = 'Create_Contact', @Id = '30E702F4-6D26-4FEA-9929-C26926CDC91C'   
                  EXECUTE updateActivityTables @Name = 'Generate_DocuSign_Report', @Id = '582A519E-7B1F-4424-B67B-EAA526C6953C'   
                  EXECUTE updateActivityTables @Name = 'FindObjects_Solution', @Id = 'B83480C3-033D-4908-BCC5-EBF98AA9816E'   
                  EXECUTE updateActivityTables @Name = 'LaunchAPlan', @Id = '18BE05F5-47CF-4736-B38C-F67AA114215D'  
                  EXECUTE updateActivityTables @Name = 'Create_Account', @Id = '0C256CBA-AAF3-482D-8AF1-FFBA153C3989'  
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_Salesforce_Event', @Id = '3cb9d14e-6756-410f-b19a-1b365eff267d'
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_Fr8_Events', @Id = 'e75112ed-e17d-4b90-a337-50a5d59b1866'
                  
                  EXECUTE updateActivityTables @Name = 'Prepare_DocuSign_Events_For_Storage', @Id = 'ba1424f9-a540-4b3e-b206-695857011a0a'
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_Channel', @Id = 'af0c038c-3adc-4372-b07e-e04b71102aa7', @Version = '2'
                  EXECUTE updateActivityTables @Name = 'Monitor_Channel', @Id = '246DF538-3B7E-4D1B-B045-72021BAA0D2D'
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_Feed_Posts', @Id = '860b8347-0e5a-41c3-9be7-73057eeca676'
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_Gmail_Inbox', @Id = 'd547401f-a4e3-47cd-9851-7fb98e16c94a'
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_DocuSign_Envelope_Activity', @Id = '68fb036f-c401-4492-a8ae-8f57eb59cc86'
                  
                  EXECUTE updateActivityTables @Name = 'Monitor_Stat_Changes', @Id = '47696645-2b77-4dad-9a0f-dd3b53f52063'
                  
                  EXECUTE updateActivityTables @Name = 'Get_Google_Sheet_Data', @Id = 'f389bea8-164c-42c8-bdc5-121d7fb93d73'
                  
                  EXECUTE updateActivityTables @Name = 'Get_Jira_Issue', @Id = 'e51bd483-bc63-49a1-a7c4-36e0a14a6235'
                  
                  EXECUTE updateActivityTables @Name = 'Query_DocuSign', @Id = '9e9e6230-727f-456a-b56d-5cfbbd6f551a', @Version = '2'
                  EXECUTE updateActivityTables @Name = 'Query_DocuSign', @Id = '62CB1D64-1A94-483C-A577-DA514F5D0CB0'
                  
                  EXECUTE updateActivityTables @Name = 'Get_File_List', @Id = '1cbc96d3-7d61-4acc-8cf0-6a3f0987b00d'
                  
                  EXECUTE updateActivityTables @Name = 'Get_Data', @Id = 'd8cf2810-87b9-43e7-a69b-a344823fd092'
                  
                  EXECUTE updateActivityTables @Name = 'Get_File_From_Fr8_Store', @Id = '82a722b5-40a6-42d7-8296-aa5239f10173'
                  
                  EXECUTE updateActivityTables @Name = 'Load_Excel_File', @Id = 'df2df85f-9364-48af-aa97-bb8adccc91d7'
                  
                  EXECUTE updateActivityTables @Name = 'Search_DocuSign_History', @Id = 'c64f4378-f259-4006-b4f1-f7e90709829e'
                  
                  EXECUTE updateActivityTables @Name = 'Store_File', @Id = '1c4f979d-bc1c-4a4a-b370-049dbacd3678'
                  
                  EXECUTE updateActivityTables @Name = 'Show_Report_Onscreen', @Id = '7fbefa24-44cf-4220-8a3b-04f4e49bbcc8', @Version ='2'
                  EXECUTE updateActivityTables @Name = 'Show_Report_Onscreen', @Id = '861C6810-1B34-4E15-A7B0-94ADCCB2F42B'
                  
                  EXECUTE updateActivityTables @Name = 'Extract_Table_Field', @Id = '033ec734-2b2d-4671-b1e5-21bd0395c8d2'
                  
                  EXECUTE updateActivityTables @Name = 'Convert_Crates', @Id = 'ddf95587-a001-4d0d-8b3f-636ac9553678'
                  
                  EXECUTE updateActivityTables @Name = 'Manage_Plan', @Id = '674dc325-f67d-46e3-99cd-6b355815f98e'
                  
                  EXECUTE updateActivityTables @Name = 'Test_Incoming_Data', @Id = '62087361-da08-44f4-9826-70f5e26a1d5a'
                  
                  EXECUTE updateActivityTables @Name = 'Get_Data_From_Fr8_Warehouse', @Id = '826bf794-7608-4194-8d5e-7350df9adf65'
                  
                  EXECUTE updateActivityTables @Name = 'App_Builder', @Id = '04390199-7cfd-4217-bf40-7671e130dc28'
                  
                  EXECUTE updateActivityTables @Name = 'Query_Fr8_Warehouse', @Id = 'ad46fa79-eb0b-4990-ad01-76ebf9d471da'
                  
                  EXECUTE updateActivityTables @Name = 'Set_Delay', @Id = '4059e018-d8a5-4927-9712-8430ffba0b73'
                  
                  EXECUTE updateActivityTables @Name = 'Set_Excel_Template', @Id = 'd6089960-a33d-4e8c-be60-8734e5f3d2fc'
                  
                  EXECUTE updateActivityTables @Name = 'Select_Fr8_Object', @Id = '6238483f-2cef-418e-bd7e-a52ddb1e01e5'
                  
                  EXECUTE updateActivityTables @Name = 'Connect_To_Sql', @Id = 'bb019231-435a-49c3-96db-ab4ae9e7fb23'
                  
                  EXECUTE updateActivityTables @Name = 'Loop', @Id = '3d5dd0c5-6702-4b59-8c18-b8e2c5955c40'
                  
                  EXECUTE updateActivityTables @Name = 'Filter_Object_List_By_Incoming_Message', @Id = '36470147-05e3-4f32-94ef-bf203b6c53af'
                  
                  EXECUTE updateActivityTables @Name = 'Save_To_Fr8_Warehouse', @Id = '005e5050-91f9-489a-ae9f-c1a182b6cffa'
                  
                  EXECUTE updateActivityTables @Name = 'Make_A_Decision', @Id = 'f52a0f0f-571c-4530-a49f-c2ff2e18eafd'
                  
                  EXECUTE updateActivityTables @Name = 'Build_Message', @Id = '36151a2a-baf3-4614-96f7-d147dd1a73cd'
                  
                  EXECUTE updateActivityTables @Name = 'Build_Query', @Id = '00dc3a6e-3c08-4918-824f-d966d5ebfa91'
                  
                  EXECUTE updateActivityTables @Name = 'Execute_Sql', @Id = '23e0576e-7c51-42a6-89f2-e954c8499ca5'
                  
                  EXECUTE updateActivityTables @Name = 'Convert_Related_Fields_Into_Table', @Id = '51e59b13-b164-4a4a-9a37-f528cb05e0fb'
                  
                  EXECUTE updateActivityTables @Name = 'Add_Payload_Manually', @Id = '315c3603-eb27-4217-a07e-f5c5a52bbfc7'
                  
                  EXECUTE updateActivityTables @Name = 'Post_To_Chatter', @Id = '5052fc23-c867-4d5a-8fbb-b6b64b5ad688', @Version = '2'
                  EXECUTE updateActivityTables @Name = 'Post_To_Chatter', @Id = 'E2250022-FA40-4FCF-9CDD-130DF6DD1984'
                  
                  EXECUTE updateActivityTables @Name = 'Post_To_Yammer', @Id = 'fa163960-901f-4105-8731-234aeb38f11d'
                  
                  EXECUTE updateActivityTables @Name = 'Publish_To_Slack', @Id = '0e21b4e8-3f08-41b1-bb6b-399ef4c2b683', @Version = '2'
                  EXECUTE updateActivityTables @Name = 'Publish_To_Slack', @Id = '4698C675-CA2C-4BE7-82F9-2421F3608E13'
                  
                  EXECUTE updateActivityTables @Name = 'Send_DocuSign_Envelope', @Id = '2b1b4d98-9eb1-4cba-8baa-a6247cd86dce', @Version = '2'
                  EXECUTE updateActivityTables @Name = 'Send_DocuSign_Envelope', @Id = '8AC0A48C-C4B5-43E4-B585-2870D814BA86'
                  
                  EXECUTE updateActivityTables @Name = 'Use_DocuSign_Template_With_New_Document', @Id = '55693341-6a95-4fc3-8848-2fc3a8101924'
                  
                  EXECUTE updateActivityTables @Name = 'Write_To_Sql_Server', @Id = '7150a1e3-a32a-4a0b-a632-42529e5fd24d'
                  
                  EXECUTE updateActivityTables @Name = 'Send_Via_Twilio', @Id = 'ddd5be71-a23c-41e3-baf0-501e34f0517b'
                  
                  EXECUTE updateActivityTables @Name = 'Save_To_Excel', @Id = 'f3c99f97-e6e2-4343-b592-6674ac5b4c16'
                  
                  EXECUTE updateActivityTables @Name = 'Create_Journal_Entry', @Id = '8d1d8407-488f-4494-a724-746c1ae4e901'
                  
                  EXECUTE updateActivityTables @Name = 'Save_Jira_Issue', @Id = 'd2e7f4b6-5e83-4f2f-b779-925547aa9542'
                  
                  EXECUTE updateActivityTables @Name = 'Save_To_Google_Sheet', @Id = '120110db-b8dd-41ca-b88e-9865db315528'
                  
                  EXECUTE updateActivityTables @Name = 'Save_To_SalesforceDotCom', @Id = '802bfcb5-f778-4187-82d3-b941a738a464'
                  
                  EXECUTE updateActivityTables @Name = 'Post_To_Timeline', @Id = '9710de37-7f5a-471a-9e94-c1ade0f71474'
                  
                  EXECUTE updateActivityTables @Name = 'Send_Email_Via_SendGrid', @Id = 'f827af1c-3348-4981-bebd-cf81c8ab27ae'
                  
                  EXECUTE updateActivityTables @Name = 'Send_SMS', @Id = '61774e73-9151-4c58-8a56-dd6653bc2e8c'
                  
                  EXECUTE updateActivityTables @Name = 'Write_To_Log', @Id = '82689803-f577-47cd-9a7a-dd728f72acfe'
                  
                  EXECUTE updateActivityTables @Name = 'Update_Stat', @Id = 'c8a29957-972d-447d-ad08-e8c66f5b62dc'
                  
                  EXECUTE updateActivityTables @Name = 'Send_Email', @Id = '6623f94f-5484-4264-b992-f00637bcdb4c'
                  
                  EXECUTE updateActivityTables @Name = 'Track_DocuSign_Recipients', @Id = '54fdef85-e47d-4762-ba2f-3eae39cd1e9b', @Version = '2'
                  EXECUTE updateActivityTables @Name = 'Track_DocuSign_Recipients', @Id = '4202F427-CD6F-497A-B852-4223B7F109E6'
                  
                  EXECUTE updateActivityTables @Name = 'Mail_Merge_Into_DocuSign', @Id = 'ccdf8156-39fb-4082-99a4-629ec5cf1b23'
                  
                  EXECUTE updateActivityTables @Name = 'Search_Fr8_Warehouse', @Id = '33f353a1-65cc-4065-9517-71ddc0a7f4e2'
                  
                  EXECUTE updateActivityTables @Name = 'Extract_Data_From_Envelopes', @Id = '9676dd67-519d-4492-ad25-b5f55f9b4804'
                  
                  EXECUTE updateActivityTables @Name = 'Mail_Merge_From_Salesforce', @Id = '81c02e05-6561-4e3e-ab10-e327a5c601e9'
                  
                  
                  
                  
                  
                  
                  
                  UPDATE ActivityTemplate SET Id = 'f7619e79-112e-43aa-ba43-118c1ffc98f3' WHERE Name = 'Monitor_Form_Responses' AND Version = '1' 
                  
                  UPDATE ActivityTemplate SET Id = '3cb9d14e-6756-410f-b19a-1b365eff267d' WHERE Name = 'Monitor_Salesforce_Event' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'e75112ed-e17d-4b90-a337-50a5d59b1866' WHERE Name = 'Monitor_Fr8_Events' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'ba1424f9-a540-4b3e-b206-695857011a0a' WHERE Name = 'Prepare_DocuSign_Events_For_Storage' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'af0c038c-3adc-4372-b07e-e04b71102aa7' WHERE Name = 'Monitor_Channel' AND Version = '2'
                  UPDATE ActivityTemplate SET Id = '246DF538-3B7E-4D1B-B045-72021BAA0D2D' WHERE Name = 'Monitor_Channel' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '860b8347-0e5a-41c3-9be7-73057eeca676' WHERE Name = 'Monitor_Feed_Posts' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'd547401f-a4e3-47cd-9851-7fb98e16c94a' WHERE Name = 'Monitor_Gmail_Inbox' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '68fb036f-c401-4492-a8ae-8f57eb59cc86' WHERE Name = 'Monitor_DocuSign_Envelope_Activity' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '47696645-2b77-4dad-9a0f-dd3b53f52063' WHERE Name = 'Monitor_Stat_Changes' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'f389bea8-164c-42c8-bdc5-121d7fb93d73' WHERE Name = 'Get_Google_Sheet_Data' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'e51bd483-bc63-49a1-a7c4-36e0a14a6235' WHERE Name = 'Get_Jira_Issue' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '9e9e6230-727f-456a-b56d-5cfbbd6f551a' WHERE Name = 'Query_DocuSign' AND Version = '2'
                  UPDATE ActivityTemplate SET Id = '62CB1D64-1A94-483C-A577-DA514F5D0CB0' WHERE Name = 'Query_DocuSign' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '1cbc96d3-7d61-4acc-8cf0-6a3f0987b00d' WHERE Name = 'Get_File_List' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'd8cf2810-87b9-43e7-a69b-a344823fd092' WHERE Name = 'Get_Data' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '82a722b5-40a6-42d7-8296-aa5239f10173' WHERE Name = 'Get_File_From_Fr8_Store' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'df2df85f-9364-48af-aa97-bb8adccc91d7' WHERE Name = 'Load_Excel_File' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'c64f4378-f259-4006-b4f1-f7e90709829e' WHERE Name = 'Search_DocuSign_History' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '1c4f979d-bc1c-4a4a-b370-049dbacd3678' WHERE Name = 'Store_File' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '7fbefa24-44cf-4220-8a3b-04f4e49bbcc8' WHERE Name = 'Show_Report_Onscreen' AND Version = '2'
                  UPDATE ActivityTemplate SET Id = '861C6810-1B34-4E15-A7B0-94ADCCB2F42B' WHERE Name = 'Show_Report_Onscreen' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '033ec734-2b2d-4671-b1e5-21bd0395c8d2' WHERE Name = 'Extract_Table_Field' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'ddf95587-a001-4d0d-8b3f-636ac9553678' WHERE Name = 'Convert_Crates' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '674dc325-f67d-46e3-99cd-6b355815f98e' WHERE Name = 'Manage_Plan' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '62087361-da08-44f4-9826-70f5e26a1d5a' WHERE Name = 'Test_Incoming_Data' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '826bf794-7608-4194-8d5e-7350df9adf65' WHERE Name = 'Get_Data_From_Fr8_Warehouse' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '04390199-7cfd-4217-bf40-7671e130dc28' WHERE Name = 'App_Builder' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'ad46fa79-eb0b-4990-ad01-76ebf9d471da' WHERE Name = 'Query_Fr8_Warehouse' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '4059e018-d8a5-4927-9712-8430ffba0b73' WHERE Name = 'Set_Delay' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'd6089960-a33d-4e8c-be60-8734e5f3d2fc' WHERE Name = 'Set_Excel_Template' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '6238483f-2cef-418e-bd7e-a52ddb1e01e5' WHERE Name = 'Select_Fr8_Object' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'bb019231-435a-49c3-96db-ab4ae9e7fb23' WHERE Name = 'Connect_To_Sql' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '3d5dd0c5-6702-4b59-8c18-b8e2c5955c40' WHERE Name = 'Loop' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '36470147-05e3-4f32-94ef-bf203b6c53af' WHERE Name = 'Filter_Object_List_By_Incoming_Message' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '005e5050-91f9-489a-ae9f-c1a182b6cffa' WHERE Name = 'Save_To_Fr8_Warehouse' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'f52a0f0f-571c-4530-a49f-c2ff2e18eafd' WHERE Name = 'Make_A_Decision' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '36151a2a-baf3-4614-96f7-d147dd1a73cd' WHERE Name = 'Build_Message' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '00dc3a6e-3c08-4918-824f-d966d5ebfa91' WHERE Name = 'Build_Query' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '23e0576e-7c51-42a6-89f2-e954c8499ca5' WHERE Name = 'Execute_Sql' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '51e59b13-b164-4a4a-9a37-f528cb05e0fb' WHERE Name = 'Convert_Related_Fields_Into_Table' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '315c3603-eb27-4217-a07e-f5c5a52bbfc7' WHERE Name = 'Add_Payload_Manually' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '5052fc23-c867-4d5a-8fbb-b6b64b5ad688' WHERE Name = 'Post_To_Chatter' AND Version = '2'
                  UPDATE ActivityTemplate SET Id = 'E2250022-FA40-4FCF-9CDD-130DF6DD1984' WHERE Name = 'Post_To_Chatter' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'fa163960-901f-4105-8731-234aeb38f11d' WHERE Name = 'Post_To_Yammer' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '0e21b4e8-3f08-41b1-bb6b-399ef4c2b683' WHERE Name = 'Publish_To_Slack' AND Version = '2'
                  UPDATE ActivityTemplate SET Id = '4698C675-CA2C-4BE7-82F9-2421F3608E13' WHERE Name = 'Publish_To_Slack' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '2b1b4d98-9eb1-4cba-8baa-a6247cd86dce' WHERE Name = 'Send_DocuSign_Envelope' AND Version = '2'
                  UPDATE ActivityTemplate SET Id = '8AC0A48C-C4B5-43E4-B585-2870D814BA86' WHERE Name = 'Send_DocuSign_Envelope' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '55693341-6a95-4fc3-8848-2fc3a8101924' WHERE Name = 'Use_DocuSign_Template_With_New_Document' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '7150a1e3-a32a-4a0b-a632-42529e5fd24d' WHERE Name = 'Write_To_Sql_Server' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'ddd5be71-a23c-41e3-baf0-501e34f0517b' WHERE Name = 'Send_Via_Twilio' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'f3c99f97-e6e2-4343-b592-6674ac5b4c16' WHERE Name = 'Save_To_Excel' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '8d1d8407-488f-4494-a724-746c1ae4e901' WHERE Name = 'Create_Journal_Entry' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'd2e7f4b6-5e83-4f2f-b779-925547aa9542' WHERE Name = 'Save_Jira_Issue' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '120110db-b8dd-41ca-b88e-9865db315528' WHERE Name = 'Save_To_Google_Sheet' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '802bfcb5-f778-4187-82d3-b941a738a464' WHERE Name = 'Save_To_SalesforceDotCom' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '9710de37-7f5a-471a-9e94-c1ade0f71474' WHERE Name = 'Post_To_Timeline' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'f827af1c-3348-4981-bebd-cf81c8ab27ae' WHERE Name = 'Send_Email_Via_SendGrid' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '61774e73-9151-4c58-8a56-dd6653bc2e8c' WHERE Name = 'Send_SMS' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '82689803-f577-47cd-9a7a-dd728f72acfe' WHERE Name = 'Write_To_Log' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'c8a29957-972d-447d-ad08-e8c66f5b62dc' WHERE Name = 'Update_Stat' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '6623f94f-5484-4264-b992-f00637bcdb4c' WHERE Name = 'Send_Email' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '54fdef85-e47d-4762-ba2f-3eae39cd1e9b' WHERE Name = 'Track_DocuSign_Recipients' AND Version = '2'
                  UPDATE ActivityTemplate SET Id = '4202F427-CD6F-497A-B852-4223B7F109E6' WHERE Name = 'Track_DocuSign_Recipients' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = 'ccdf8156-39fb-4082-99a4-629ec5cf1b23' WHERE Name = 'Mail_Merge_Into_DocuSign' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '33f353a1-65cc-4065-9517-71ddc0a7f4e2' WHERE Name = 'Search_Fr8_Warehouse' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '9676dd67-519d-4492-ad25-b5f55f9b4804' WHERE Name = 'Extract_Data_From_Envelopes' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '81c02e05-6561-4e3e-ab10-e327a5c601e9' WHERE Name = 'Mail_Merge_From_Salesforce' AND Version = '1'
                  
                  UPDATE ActivityTemplate SET Id = '9913A4C4-18A7-4423-AACE-02487BA8C21B' WHERE Name = 'FilterUsingRunTimeData'                                           
                  UPDATE ActivityTemplate SET Id = '1C4F979D-BC1C-4A4A-B370-049DBACD3678' WHERE Name = 'Store_File'                                          
                  UPDATE ActivityTemplate SET Id = 'CE88F864-24D1-4362-BD9E-066A40B028B6' WHERE Name = 'Archive_DocuSign_Template'                                            
                  UPDATE ActivityTemplate SET Id = '0DE0F1FC-EBD3-48A6-9DF4-06F396E9F8C3' WHERE Name = 'Get_DocuSign_Envelope'                                            
                  UPDATE ActivityTemplate SET Id = '36E485D9-FC8D-4C37-8593-226E5FD553D4' WHERE Name = 'Extract_Data'                                            
                  UPDATE ActivityTemplate SET Id = '86A28016-9BDE-4E62-A62D-3D48BB994C2D' WHERE Name = 'PlanLauncher'                                            
                  UPDATE ActivityTemplate SET Id = 'B0BDC517-1E5C-4AF8-BCA6-4916328F94E5' WHERE Name = 'Receive_DocuSign_Envelope'                                            
                  UPDATE ActivityTemplate SET Id = 'BC16B3E3-F832-4D8B-A72C-51A401F53CA2' WHERE Name = 'Extract_Spreadsheet_Data'                                          
                  UPDATE ActivityTemplate SET Id = '6D7EE641-0916-44C5-8846-54104D9A837E' WHERE Name = 'CollectData'                                            
                  UPDATE ActivityTemplate SET Id = '5E92E326-06E3-4C5B-A1F9-7542E8CD7C07' WHERE Name = 'Get_DocuSign_Template'                                            
                  UPDATE ActivityTemplate SET Id = '256DFEB9-4B98-424F-A006-7612684E3FE8' WHERE Name = 'Record_DocuSign_Events'                                            
                  UPDATE ActivityTemplate SET Id = '747FD7FD-FB3B-4D5D-9348-7BDF08C32A92' WHERE Name = 'ManageRoute'                                              
                  UPDATE ActivityTemplate SET Id = '5D925036-AC68-41D5-9646-9853F04B05CC' WHERE Name = 'Process_Personal_Report'                                             
                  UPDATE ActivityTemplate SET Id = 'F0A61DD1-D1D0-4580-BEDE-9AF066F8E896' WHERE Name = 'MapFields'                                             
                  UPDATE ActivityTemplate SET Id = 'B41DD807-4375-410E-AF19-D2F5C508004F' WHERE Name = 'Process_Personnel_Report'                                             
                  UPDATE ActivityTemplate SET Id = '83313A48-85FD-4EF3-A377-DAA450705C69' WHERE Name = 'Convert_TableData_To_AccountingTransactions'                                             
                  UPDATE ActivityTemplate SET Id = '30E702F4-6D26-4FEA-9929-C26926CDC91C' WHERE Name = 'Create_Contact'                                             
                  UPDATE ActivityTemplate SET Id = '582A519E-7B1F-4424-B67B-EAA526C6953C' WHERE Name = 'Generate_DocuSign_Report'                                             
                  UPDATE ActivityTemplate SET Id = 'B83480C3-033D-4908-BCC5-EBF98AA9816E' WHERE Name = 'FindObjects_Solution'                                             
                  UPDATE ActivityTemplate SET Id = '18BE05F5-47CF-4736-B38C-F67AA114215D' WHERE Name = 'LaunchAPlan'                                            
                  UPDATE ActivityTemplate SET Id = '0C256CBA-AAF3-482D-8AF1-FFBA153C3989' WHERE Name = 'Create_Account' 
                  
                  
                  
                  
                  
                  
                  ALTER TABLE ActivityCategorySet WITH CHECK CHECK CONSTRAINT [FK_dbo.ActivityCategorySet_dbo.ActivityTemplate_ActivityTemplateId]
                  
                  ALTER TABLE ActivityDescriptions WITH CHECK CHECK CONSTRAINT [FK_dbo.ActivityDescriptions_dbo.ActivityTemplate_ActivityTemplateId]
                  
                  ALTER TABLE Actions WITH CHECK CHECK CONSTRAINT [FK_dbo.Actions_dbo.ActivityTemplate_ActivityTemplateId]
                  END   
                  ELSE
                  PRINT N'ActivityTemplateIds already up to date'               
                  ");
            Sql(@"DROP PROCEDURE  updateActivityTables");
            Sql(@"DROP PROCEDURE  updateActivity");
            Sql(@"DROP PROCEDURE  updateActivityDescription");
            Sql(@"DROP PROCEDURE  updateActivityCategorySet");
        }
        
        public override void Down()
        {
        }
    }
}
