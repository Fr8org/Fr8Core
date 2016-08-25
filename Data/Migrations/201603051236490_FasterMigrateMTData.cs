namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class FasterMigrateMTData : System.Data.Entity.Migrations.DbMigration
    {
        const string SqlScript = @"
        IF OBJECT_ID('tempdb.dbo.#MtTypes', 'U') IS NOT NULL
	drop table #MtTypes

IF OBJECT_ID('tempdb.dbo.#PropMapping', 'U') IS NOT NULL
	drop table #PropMapping 

IF OBJECT_ID('tempdb.dbo.#MtProperties', 'U') IS NOT NULL
	drop table #MtProperties

IF OBJECT_ID('tempdb.dbo.#TypeMapping', 'U') IS NOT NULL
	drop table #TypeMapping

IF OBJECT_ID('tempdb.dbo.#tempData', 'U') IS NOT NULL
	drop table #tempData

IF OBJECT_ID('tempdb.dbo.#tfields', 'U') IS NOT NULL
	drop table #tfields

IF OBJECT_ID('tempdb.dbo.#tempData2', 'U') IS NOT NULL
	drop table #tempData2

	IF OBJECT_ID('tempdb.dbo.#mapStr', 'U') IS NOT NULL
	drop table #mapStr

CREATE TABLE #MtTypes (Id uniqueidentifier, Alias nvarchar(300), Name Char( 255 ), IsPrimitive bit, IsComplex bit, ManifestId int );
CREATE TABLE #MtProperties (Name nvarchar (100), DeclaringType uniqueidentifier, PropType uniqueidentifier, offset int);

insert into #MtTypes values ('09932a02-9bac-4fd7-8384-30a5bb1cb9aa', NULL, 'System.String', 1, 0, NULL)
insert into #MtTypes values ('3cbcdd61-846e-4aae-a789-b053575d5eaf', NULL, 'System.Boolean', 1, 0, NULL)
insert into #MtTypes values ('6cc89c99-c67e-4ba9-9030-027d8d433b10', NULL, 'System.Nullable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('321770b7-0781-4a93-9f38-78b0bc7d5a83', NULL, 'System.Byte', 1, 0, NULL)
insert into #MtTypes values ('73f0ffea-1f1e-46c0-8fec-92cb652c7cfa', NULL, 'System.Nullable`1[[System.Byte, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('640473bf-1f54-413a-a722-ee004f40a477', NULL, 'System.Char', 1, 0, NULL)
insert into #MtTypes values ('065d8f6c-b4a2-4c16-b046-11ed805c757f', NULL, 'System.Nullable`1[[System.Char, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('48cf73c3-8a5a-4ec6-adae-5e45b1a75a48', NULL, 'System.Int16', 1, 0, NULL)
insert into #MtTypes values ('234bb426-5eb7-4bf8-9f9a-d2570514f0ae', NULL, 'System.Nullable`1[[System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('7e4d96cb-543a-4898-aead-bb4e302bfe38', NULL, 'System.Int32', 1, 0, NULL)
insert into #MtTypes values ('210b5ab2-47a5-40e0-b565-223a0e906c03', NULL, 'System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('09037ff8-f7b2-44b4-9bda-51cf2f06e8aa', NULL, 'System.Int64', 1, 0, NULL)
insert into #MtTypes values ('c8a864df-ccc3-4d61-b45b-a3d5919f9d8c', NULL, 'System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('fc0963d4-8a00-4c67-bdac-97fa872be6e1', NULL, 'System.Single', 1, 0, NULL)
insert into #MtTypes values ('2dae917b-1310-4ef9-8768-f23eefa43570', NULL, 'System.Nullable`1[[System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('e4f9e25b-a822-49e9-b084-30d73385c1e0', NULL, 'System.Double', 1, 0, NULL)
insert into #MtTypes values ('291c7b7c-caae-499e-a925-2f0a71e14556', NULL, 'System.Nullable`1[[System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('390f04b2-07e4-4bbd-8a7f-738f346750b5', NULL, 'System.DateTime', 1, 0, NULL)
insert into #MtTypes values ('9de39ffc-8f5d-4b8f-8708-cc5a252f7b27', NULL, 'System.Nullable`1[[System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 1, 0, NULL)
insert into #MtTypes values ('2a40de0b-3515-4d11-a47d-5f0805682f93', 'Crate Description', 'Data.Interfaces.Manifests.CrateDescriptionCM', 0, 0, 32)
insert into #MtProperties values ('CrateDescriptions', '2a40de0b-3515-4d11-a47d-5f0805682f93', '5aa7c98f-b658-4a97-9447-781d26e3c748',  0)
insert into #MtTypes values ('5aa7c98f-b658-4a97-9447-781d26e3c748', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.CrateDescriptionDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('137d9138-18a0-443f-b60b-f57ffc899866', 'Manifest Description', 'Data.Interfaces.Manifests.ManifestDescriptionCM', 0, 0, 30)
insert into #MtProperties values ('Id', '137d9138-18a0-443f-b60b-f57ffc899866', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('Name', '137d9138-18a0-443f-b60b-f57ffc899866', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('Version', '137d9138-18a0-443f-b60b-f57ffc899866', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('SampleJSON', '137d9138-18a0-443f-b60b-f57ffc899866', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('Description', '137d9138-18a0-443f-b60b-f57ffc899866', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  4)
insert into #MtProperties values ('RegisteredBy', '137d9138-18a0-443f-b60b-f57ffc899866', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)
insert into #MtTypes values ('4e03a62a-1bb8-46bd-bb12-dc578c3a54c0', 'Chart Of Accounts', 'Data.Interfaces.Manifests.ChartOfAccountsCM', 0, 0, 29)
insert into #MtProperties values ('Accounts', '4e03a62a-1bb8-46bd-bb12-dc578c3a54c0', '21f54d21-4d04-47e2-983a-eb1349c3e28f',  0)
insert into #MtTypes values ('21f54d21-4d04-47e2-983a-eb1349c3e28f', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.Manifests.AccountDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('7295d69d-d96c-4062-8ffe-e26bce167b18', 'Docusign Template', 'Data.Interfaces.Manifests.DocuSignTemplateCM', 0, 0, 28)
insert into #MtProperties values ('Status', '7295d69d-d96c-4062-8ffe-e26bce167b18', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('CreateDate', '7295d69d-d96c-4062-8ffe-e26bce167b18', '9de39ffc-8f5d-4b8f-8708-cc5a252f7b27',  1)
insert into #MtProperties values ('Body', '7295d69d-d96c-4062-8ffe-e26bce167b18', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('Name', '7295d69d-d96c-4062-8ffe-e26bce167b18', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtTypes values ('ba616a30-a8a3-4804-8db1-05c34081a64a', 'Docusign Recipient', 'Data.Interfaces.Manifests.DocuSignRecipientCM', 0, 0, 26)
insert into #MtProperties values ('Object', 'ba616a30-a8a3-4804-8db1-05c34081a64a', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('Status', 'ba616a30-a8a3-4804-8db1-05c34081a64a', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('DocuSignAccountId', 'ba616a30-a8a3-4804-8db1-05c34081a64a', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('RecipientId', 'ba616a30-a8a3-4804-8db1-05c34081a64a', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('RecipientEmail', 'ba616a30-a8a3-4804-8db1-05c34081a64a', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  4)
insert into #MtProperties values ('EnvelopeId', 'ba616a30-a8a3-4804-8db1-05c34081a64a', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)
insert into #MtTypes values ('68fde5e8-dabe-4de1-929d-e9d1948806f4', 'Standard Accounting Transaction', 'Data.Interfaces.Manifests.StandardAccountingTransactionCM', 0, 0, 25)
insert into #MtProperties values ('AccountingTransactions', '68fde5e8-dabe-4de1-929d-e9d1948806f4', 'ae31e222-bf61-487b-af0a-6d7415f11499',  0)
insert into #MtTypes values ('ae31e222-bf61-487b-af0a-6d7415f11499', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.StandardAccountingTransactionDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('c7da943f-ed9b-4e35-8821-40adf73def85', 'Standard File List', 'Data.Interfaces.Manifests.StandardFileListCM', 0, 0, 24)
insert into #MtProperties values ('FileList', 'c7da943f-ed9b-4e35-8821-40adf73def85', 'b7dd632f-8079-4596-b64d-4e32a9ea100a',  0)
insert into #MtTypes values ('b7dd632f-8079-4596-b64d-4e32a9ea100a', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.Manifests.StandardFileDescriptionCM, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('03f98f8d-0c5e-4a11-9b73-97e67ea61291', 'Standard Fr8 Routes', 'Data.Interfaces.Manifests.StandardFr8RoutesCM', 0, 0, 19)
insert into #MtProperties values ('CreateDate', '03f98f8d-0c5e-4a11-9b73-97e67ea61291', '390f04b2-07e4-4bbd-8a7f-738f346750b5',  0)
insert into #MtProperties values ('LastUpdated', '03f98f8d-0c5e-4a11-9b73-97e67ea61291', '390f04b2-07e4-4bbd-8a7f-738f346750b5',  1)
insert into #MtProperties values ('Description', '03f98f8d-0c5e-4a11-9b73-97e67ea61291', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('Name', '03f98f8d-0c5e-4a11-9b73-97e67ea61291', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('Ordering', '03f98f8d-0c5e-4a11-9b73-97e67ea61291', '7e4d96cb-543a-4898-aead-bb4e302bfe38',  4)
insert into #MtProperties values ('RouteState', '03f98f8d-0c5e-4a11-9b73-97e67ea61291', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)
insert into #MtProperties values ('SubRoutes', '03f98f8d-0c5e-4a11-9b73-97e67ea61291', 'cb569547-7ea3-4008-b483-68b37a67606f',  6)
insert into #MtTypes values ('cb569547-7ea3-4008-b483-68b37a67606f', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.SubrouteDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', 'Docusign Envelope', 'Data.Interfaces.Manifests.DocuSignEnvelopeCM', 0, 0, 15)
insert into #MtProperties values ('Status', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('CreateDate', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '9de39ffc-8f5d-4b8f-8708-cc5a252f7b27',  1)
insert into #MtProperties values ('SentDate', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '9de39ffc-8f5d-4b8f-8708-cc5a252f7b27',  2)
insert into #MtProperties values ('DeliveredDate', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '9de39ffc-8f5d-4b8f-8708-cc5a252f7b27',  3)
insert into #MtProperties values ('CompletedDate', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '9de39ffc-8f5d-4b8f-8708-cc5a252f7b27',  4)
insert into #MtProperties values ('EnvelopeId', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)
insert into #MtProperties values ('ExternalAccountId', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  6)
insert into #MtProperties values ('StatusChangedDateTime', '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3', '9de39ffc-8f5d-4b8f-8708-cc5a252f7b27',  7)
insert into #MtTypes values ('434d61a1-e8f9-4ef5-9aad-05427842dfd8', 'Docusign Event', 'Data.Interfaces.Manifests.DocuSignEventCM', 0, 0, 14)
insert into #MtProperties values ('Object', '434d61a1-e8f9-4ef5-9aad-05427842dfd8', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('Status', '434d61a1-e8f9-4ef5-9aad-05427842dfd8', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('EventId', '434d61a1-e8f9-4ef5-9aad-05427842dfd8', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('EnvelopeId', '434d61a1-e8f9-4ef5-9aad-05427842dfd8', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('RecepientId', '434d61a1-e8f9-4ef5-9aad-05427842dfd8', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  4)
insert into #MtProperties values ('ExternalAccountId', '434d61a1-e8f9-4ef5-9aad-05427842dfd8', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)
insert into #MtTypes values ('7fb1fe4b-a108-4c6d-923c-00809805f47c', 'Standard Fr8 Terminal', 'Data.Interfaces.Manifests.StandardFr8TerminalCM', 0, 0, 23)
insert into #MtProperties values ('Definition', '7fb1fe4b-a108-4c6d-923c-00809805f47c', '20a4ae28-135f-4c51-beb8-805ef550bd8b',  0)
insert into #MtProperties values ('Activities', '7fb1fe4b-a108-4c6d-923c-00809805f47c', '34409e71-9f89-4073-a679-0230e126fd07',  1)
insert into #MtTypes values ('20a4ae28-135f-4c51-beb8-805ef550bd8b', NULL, 'Data.Interfaces.DataTransferObjects.TerminalDTO', 0, 1, NULL)
insert into #MtTypes values ('34409e71-9f89-4073-a679-0230e126fd07', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.ActivityTemplateDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('a4a4a76b-18c2-4de0-8f04-e6d39952d19e', 'Standard Authentication', 'Data.Interfaces.Manifests.StandardAuthenticationCM', 0, 0, 12)
insert into #MtProperties values ('Mode', 'a4a4a76b-18c2-4de0-8f04-e6d39952d19e', '3c35bee0-d2c7-486c-8919-f41c3a0f481d',  0)
insert into #MtTypes values ('3c35bee0-d2c7-486c-8919-f41c3a0f481d', NULL, 'Data.Interfaces.Manifests.AuthenticationMode', 0, 1, NULL)
insert into #MtTypes values ('9e909f75-aacd-4e9c-9142-155f54bd0fb0', 'Standard UI Controls', 'Data.Interfaces.Manifests.StandardConfigurationControlsCM', 0, 0, 6)
insert into #MtProperties values ('Controls', '9e909f75-aacd-4e9c-9142-155f54bd0fb0', '9ef20759-45fb-4897-a9eb-532600bc8cf8',  0)
insert into #MtTypes values ('9ef20759-45fb-4897-a9eb-532600bc8cf8', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.ControlDefinitionDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('46c5839f-aa07-4a44-8edf-2b1059eb2a8d', 'Standard Email Message', 'Data.Interfaces.Manifests.StandardEmailMessageCM', 0, 0, 18)
insert into #MtProperties values ('MessageID', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('References', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('Subject', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('HtmlText', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('PlainText', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  4)
insert into #MtProperties values ('DateReceived', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '390f04b2-07e4-4bbd-8a7f-738f346750b5',  5)
insert into #MtProperties values ('EmailStatus', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  6)
insert into #MtProperties values ('EmailFromName', '46c5839f-aa07-4a44-8edf-2b1059eb2a8d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  7)
insert into #MtTypes values ('a28b5f81-ff5b-4269-b86c-ff4b28a99f4b', 'Standard Fr8 Containers', 'Data.Interfaces.Manifests.StandardFr8ContainersCM', 0, 0, 21)
insert into #MtProperties values ('Name', 'a28b5f81-ff5b-4269-b86c-ff4b28a99f4b', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('Description', 'a28b5f81-ff5b-4269-b86c-ff4b28a99f4b', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('CreatedDate', 'a28b5f81-ff5b-4269-b86c-ff4b28a99f4b', '390f04b2-07e4-4bbd-8a7f-738f346750b5',  2)
insert into #MtProperties values ('LastUpdated', 'a28b5f81-ff5b-4269-b86c-ff4b28a99f4b', '390f04b2-07e4-4bbd-8a7f-738f346750b5',  3)
insert into #MtTypes values ('44f8ae01-bcad-47e2-aab7-d10fd281fbb3', 'Standard Fr8 Hubs', 'Data.Interfaces.Manifests.StandardFr8HubsCM', 0, 0, 20)
insert into #MtProperties values ('Name', '44f8ae01-bcad-47e2-aab7-d10fd281fbb3', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('Description', '44f8ae01-bcad-47e2-aab7-d10fd281fbb3', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('CreatedDate', '44f8ae01-bcad-47e2-aab7-d10fd281fbb3', '390f04b2-07e4-4bbd-8a7f-738f346750b5',  2)
insert into #MtProperties values ('LastUpdated', '44f8ae01-bcad-47e2-aab7-d10fd281fbb3', '390f04b2-07e4-4bbd-8a7f-738f346750b5',  3)
insert into #MtTypes values ('7b111a3d-2c67-4e21-b9c5-987f0173a66d', 'Standard Logging Crate', 'Data.Interfaces.Manifests.StandardLoggingCM', 0, 0, 13)
insert into #MtProperties values ('Item', '7b111a3d-2c67-4e21-b9c5-987f0173a66d', '60ca2263-7c58-4e7b-a71a-de7402537fe9',  0)
insert into #MtProperties values ('LoggingMTkey', '7b111a3d-2c67-4e21-b9c5-987f0173a66d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtTypes values ('60ca2263-7c58-4e7b-a71a-de7402537fe9', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.LogItemDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('3ff6b274-39f7-4f5c-8457-02dda23ac00c', 'Standard Parsing Record', 'Data.Interfaces.Manifests.StandardParsingRecordCM', 0, 0, 22)
insert into #MtProperties values ('Name', '3ff6b274-39f7-4f5c-8457-02dda23ac00c', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('StartDate', '3ff6b274-39f7-4f5c-8457-02dda23ac00c', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('EndDate', '3ff6b274-39f7-4f5c-8457-02dda23ac00c', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('Service', '3ff6b274-39f7-4f5c-8457-02dda23ac00c', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('ExternalAccountId', '3ff6b274-39f7-4f5c-8457-02dda23ac00c', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  4)
insert into #MtProperties values ('InternalAccountId', '3ff6b274-39f7-4f5c-8457-02dda23ac00c', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)
insert into #MtTypes values ('4bfa98c6-ab23-43dc-9a23-1f0207dd5455', 'Standard Query Crate', 'Data.Interfaces.Manifests.StandardQueryCM', 0, 0, 17)
insert into #MtProperties values ('Queries', '4bfa98c6-ab23-43dc-9a23-1f0207dd5455', '9d52d4d2-519a-40c2-8f49-b06c53a06ac2',  0)
insert into #MtTypes values ('9d52d4d2-519a-40c2-8f49-b06c53a06ac2', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.QueryDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('027f1843-a3a5-4d25-9688-51ab5960d883', 'Standard Query Fields', 'Data.Interfaces.Manifests.StandardQueryFieldsCM', 0, 0, 31)
insert into #MtProperties values ('Fields', '027f1843-a3a5-4d25-9688-51ab5960d883', 'b6640ae1-2b08-4ce0-835b-44c128004766',  0)
insert into #MtTypes values ('b6640ae1-2b08-4ce0-835b-44c128004766', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.QueryFieldDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('3d3ad291-322a-4c34-b8b3-a227a56ac78f', 'Standard Routing Directive', 'Data.Interfaces.Manifests.StandardRoutingDirectiveCM', 0, 0, 11)
insert into #MtProperties values ('Directive', '3d3ad291-322a-4c34-b8b3-a227a56ac78f', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('TargetProcessNodeName', '3d3ad291-322a-4c34-b8b3-a227a56ac78f', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('TargetActivityName', '3d3ad291-322a-4c34-b8b3-a227a56ac78f', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('Explanation', '3d3ad291-322a-4c34-b8b3-a227a56ac78f', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtTypes values ('02f1fe6b-7504-4613-a7bf-32922d2f65f2', 'Standard File Handle', 'Data.Interfaces.Manifests.StandardFileDescriptionCM', 0, 0, 10)
insert into #MtProperties values ('DirectUrl', '02f1fe6b-7504-4613-a7bf-32922d2f65f2', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('Filename', '02f1fe6b-7504-4613-a7bf-32922d2f65f2', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('Filetype', '02f1fe6b-7504-4613-a7bf-32922d2f65f2', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('TextRepresentation', '02f1fe6b-7504-4613-a7bf-32922d2f65f2', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtTypes values ('e1ae8087-0dc5-4d8b-8d11-478e7012cd11', 'Operational State', 'Data.Interfaces.Manifests.OperationalStateCM', 0, 0, 27)
insert into #MtProperties values ('Loops', 'e1ae8087-0dc5-4d8b-8d11-478e7012cd11', 'b8105cdc-bccd-4ebe-b6d6-41f83bfa0ea0',  0)
insert into #MtProperties values ('CurrentActivityErrorCode', 'e1ae8087-0dc5-4d8b-8d11-478e7012cd11', '9a23a5e1-e74d-4838-b785-d4061627a1b3',  1)
insert into #MtProperties values ('CurrentActivityErrorMessage', 'e1ae8087-0dc5-4d8b-8d11-478e7012cd11', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('CurrentClientActivityName', 'e1ae8087-0dc5-4d8b-8d11-478e7012cd11', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('CurrentActivityResponse', 'e1ae8087-0dc5-4d8b-8d11-478e7012cd11', '82407eea-5666-463c-a542-8f3278e64304',  4)
insert into #MtTypes values ('b8105cdc-bccd-4ebe-b6d6-41f83bfa0ea0', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.Manifests.OperationalStateCM+LoopStatus, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('9a23a5e1-e74d-4838-b785-d4061627a1b3', NULL, 'System.Nullable`1[[Data.Constants.ActivityErrorCode, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('82407eea-5666-463c-a542-8f3278e64304', NULL, 'Data.Interfaces.DataTransferObjects.ActivityResponseDTO', 0, 1, NULL)
insert into #MtTypes values ('b9b819d1-a312-4a8a-98d2-ce9582635c59', 'Standard Security Crate', 'Data.Interfaces.Manifests.StandardSecurityCM', 0, 0, 16)
insert into #MtProperties values ('AuthenticateAs', 'b9b819d1-a312-4a8a-98d2-ce9582635c59', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtTypes values ('01246116-d0a6-4fe1-b097-c33dbf2ffec0', 'Standard Table Data', 'Data.Interfaces.Manifests.StandardTableDataCM', 0, 0, 9)
insert into #MtProperties values ('Table', '01246116-d0a6-4fe1-b097-c33dbf2ffec0', 'c279e16c-ea83-4c47-bcd4-b3fc100190f9',  0)
insert into #MtProperties values ('FirstRowHeaders', '01246116-d0a6-4fe1-b097-c33dbf2ffec0', '3cbcdd61-846e-4aae-a789-b053575d5eaf',  1)
insert into #MtTypes values ('c279e16c-ea83-4c47-bcd4-b3fc100190f9', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.Manifests.TableRowDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('dce426de-b374-470b-b266-d129ed01f565', 'Standard Payload Data', 'Data.Interfaces.Manifests.StandardPayloadDataCM', 0, 0, 5)
insert into #MtProperties values ('Name', 'dce426de-b374-470b-b266-d129ed01f565', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('PayloadObjects', 'dce426de-b374-470b-b266-d129ed01f565', '0a61b0c7-1e98-4463-adbb-ddfec9b75484',  1)
insert into #MtProperties values ('ObjectType', 'dce426de-b374-470b-b266-d129ed01f565', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtTypes values ('0a61b0c7-1e98-4463-adbb-ddfec9b75484', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.Manifests.PayloadObjectDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('a78f97dc-9a0a-4a0e-92d3-27a67a62bc2c', 'Field Description', 'Data.Interfaces.Manifests.FieldDescriptionsCM', 0, 0, 3)
insert into #MtProperties values ('Fields', 'a78f97dc-9a0a-4a0e-92d3-27a67a62bc2c', '29b15dbd-d133-4a1c-bd7c-7e36c9222600',  0)
insert into #MtTypes values ('29b15dbd-d133-4a1c-bd7c-7e36c9222600', NULL, 'System.Collections.Generic.List`1[[Data.Interfaces.DataTransferObjects.FieldDTO, Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]', 0, 1, NULL)
insert into #MtTypes values ('1e3881ac-ba39-4792-b0a9-aed6d23edace', 'Standard Event Report', 'Data.Interfaces.Manifests.EventReportCM', 0, 0, 7)
insert into #MtProperties values ('EventNames', '1e3881ac-ba39-4792-b0a9-aed6d23edace', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('ContainerDoId', '1e3881ac-ba39-4792-b0a9-aed6d23edace', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('ExternalAccountId', '1e3881ac-ba39-4792-b0a9-aed6d23edace', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('EventPayload', '1e3881ac-ba39-4792-b0a9-aed6d23edace', 'eecfc376-b3bf-4795-8c27-21741d1cf3c1',  3)
insert into #MtProperties values ('Manufacturer', '1e3881ac-ba39-4792-b0a9-aed6d23edace', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  4)
insert into #MtProperties values ('Source', '1e3881ac-ba39-4792-b0a9-aed6d23edace', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)
insert into #MtTypes values ('eecfc376-b3bf-4795-8c27-21741d1cf3c1', NULL, 'Data.Crates.ICrateStorage', 0, 1, NULL)
insert into #MtTypes values ('5ae9b9f0-ace5-4705-a885-0d2f3d32fd6f', 'Standard Event Subscription', 'Data.Interfaces.Manifests.EventSubscriptionCM', 0, 0, 8)
insert into #MtProperties values ('Subscriptions', '5ae9b9f0-ace5-4705-a885-0d2f3d32fd6f', '0c5954c9-2e97-4fa5-be0c-8958e584cf87',  0)
insert into #MtProperties values ('Manufacturer', '5ae9b9f0-ace5-4705-a885-0d2f3d32fd6f', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtTypes values ('0c5954c9-2e97-4fa5-be0c-8958e584cf87', NULL, 'System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]', 0, 1, NULL)
insert into #MtTypes values ('7cc02f22-631f-4097-a3de-d49284d2476d', 'Dockyard Terminal Event or Incident Report', 'Data.Interfaces.DataTransferObjects.EventCM', 0, 0, 2)
insert into #MtProperties values ('EventName', '7cc02f22-631f-4097-a3de-d49284d2476d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('PalletId', '7cc02f22-631f-4097-a3de-d49284d2476d', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('CrateStorage', '7cc02f22-631f-4097-a3de-d49284d2476d', 'eecfc376-b3bf-4795-8c27-21741d1cf3c1',  2)
insert into #MtTypes values ('c5d060aa-2100-4432-8d39-388f541b0105', 'Logging Data', 'Data.Interfaces.DataTransferObjects.LoggingDataCm', 0, 0, 10013)
insert into #MtProperties values ('ObjectId', 'c5d060aa-2100-4432-8d39-388f541b0105', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  0)
insert into #MtProperties values ('CustomerId', 'c5d060aa-2100-4432-8d39-388f541b0105', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  1)
insert into #MtProperties values ('Data', 'c5d060aa-2100-4432-8d39-388f541b0105', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  2)
insert into #MtProperties values ('PrimaryCategory', 'c5d060aa-2100-4432-8d39-388f541b0105', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  3)
insert into #MtProperties values ('SecondaryCategory', 'c5d060aa-2100-4432-8d39-388f541b0105', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  4)
insert into #MtProperties values ('Activity', 'c5d060aa-2100-4432-8d39-388f541b0105', '09932a02-9bac-4fd7-8384-30a5bb1cb9aa',  5)

select MT_Fields.* into #tfields from  MT_fields  inner join   (select max(Id) as Id from Mt_Fields group by MT_ObjectId, Name) as t on  t.Id = MT_Fields.ID
delete from Mt_Fields
insert into Mt_fields ([Name]
      ,[FieldColumnOffset]
      ,[MT_ObjectId]
      ,[MT_FieldType_Id]) 

select [Name]
      ,[FieldColumnOffset]
      ,[MT_ObjectId]
      ,[MT_FieldType_Id] from #tfields
drop table #tfields

select ft.Id as LegacyId, case when kt.Id is null then NEWID() else kt.Id end as Id, kt.Alias, ft.TypeName +', ' + ft.AssemblyName as ClrName, kt.IsPrimitive, case when kt.IsComplex is null then 1 else kt.IsComplex end as IsComplex, kt.ManifestId 
into #TypeMapping from MT_FieldType as ft 
left join #MtTypes as kt on ft.TypeName = kt.Name

 
select ROW_NUMBER() 
        OVER (ORDER BY MT_Fields.Name) as Id, #MtProperties.DeclaringType, #MtProperties.PropType as PropertyType, MT_Fields.MT_ObjectId,  MT_Fields.Name, MT_Fields.FieldColumnOffset as LegacyOffset, #MtProperties.offset as Offset into #PropMapping from MT_Fields 
inner join MT_objects on MT_Fields.MT_ObjectId = Mt_Objects.Id
inner join #TypeMapping on #TypeMapping.LegacyId = Mt_Objects.MT_FieldType_Id 
inner join #MtProperties on #MtProperties.Name = MT_Fields.Name and #TypeMapping.Id =  #MtProperties.DeclaringType

delete from MtTypes
delete from MtProperties
delete from MtData

insert into MtTypes (Id, Alias, ClrName, IsPrimitive, IsComplex, ManifestId) 
select Id,Alias, ClrName, case when IsPrimitive is null then 0 else IsPrimitive end, case when IsComplex is null then 0 else IsComplex end, ManifestId from #TypeMapping
where #TypeMapping.LegacyId  in (select max(LegacyId) from #TypeMapping  group by Id)

insert into MtProperties (Name, Offset, DeclaringType, Type)
select Name, Offset, DeclaringType, PropertyType from #PropMapping where #PropMapping.Id  in (select max(Id) from #PropMapping group by DeclaringType, Name)

Select distinct pm.DeclaringType,
	(
		SELECT STUFF((SELECT case when #PropMapping.PropertyType = '390f04b2-07e4-4bbd-8a7f-738f346750b5' or #PropMapping.PropertyType = '9de39ffc-8f5d-4b8f-8708-cc5a252f7b27' then
									', Convert(nvarchar, Convert(datetimeoffset, Value' + Convert(nvarchar, (#PropMapping.Offset+1)) + '), 127)'
								else
									', Value' + Convert(nvarchar, (#PropMapping.Offset+1)) 
								end
		FROM #PropMapping where pm.DeclaringType = #PropMapping.DeclaringType and #PropMapping.Id  in (select max(Id) from #PropMapping group by DeclaringType, Name) order by Id
		FOR XML PATH('')) ,1,1,'') AS Txt
	) as new, 
			
	(
			SELECT STUFF((SELECT ', Value' + Convert(nvarchar, (#PropMapping.LegacyOffset)) 
						  
        FROM #PropMapping where pm.DeclaringType = #PropMapping.DeclaringType and #PropMapping.Id  in (select max(Id) from #PropMapping group by DeclaringType, Name) order by Id
        FOR XML PATH('')) ,1,1,'') AS Txt
	)  as legacy
    into #mapStr			
	from #PropMapping as pm

declare @script nvarchar(max)

declare cur CURSOR LOCAL for
   select 'insert into MtData (Type, CreatedAt, UpdatedAt, fr8AccountId, IsDeleted, ' + legacy + ') select ''' + Convert(nvarchar(max), #TypeMapping.Id)  +''', CreatedAt, UpdatedAt, fr8AccountId, IsDeleted, ' + new + ' from Mt_data where MT_ObjectId = ' + Convert(nvarchar(max), MT_Objects.Id) as script from #mapStr
	inner join #TypeMapping on DeclaringType = #TypeMapping.Id
	inner join MT_Objects on #TypeMapping.LegacyId  = MT_Objects.MT_FieldType_Id

open cur

fetch next from cur into @script

while @@FETCH_STATUS = 0 BEGIN
	exec (@script)
    fetch next from cur into @script
END

close cur
deallocate cur

;WITH cte AS (
 SELECT ROW_NUMBER() OVER 
          (PARTITION BY Value6, Fr8AccountId ORDER BY Id DESC) AS sequence
    FROM MtData where Type = '0014d784-82a2-4e23-9ab2-dcf6a6f9e2a3'
)
DELETE
FROM cte
WHERE sequence > 1

;WITH cte AS (
 SELECT ROW_NUMBER() OVER 
          (PARTITION BY Value6, Fr8AccountId ORDER BY Id DESC) AS sequence
    FROM MtData where Type = 'BA616A30-A8A3-4804-8DB1-05C34081A64A'
)
DELETE
FROM cte
WHERE sequence > 1

;WITH cte AS (
 SELECT ROW_NUMBER() OVER 
          (PARTITION BY Value4, Fr8AccountId ORDER BY Id DESC) AS sequence
    FROM MtData where Type = '434D61A1-E8F9-4EF5-9AAD-05427842DFD8'
)
DELETE
FROM cte
WHERE sequence > 1

;WITH cte AS (
 SELECT ROW_NUMBER() OVER 
          (PARTITION BY Value2, Fr8AccountId ORDER BY Id DESC) AS sequence
    FROM MtData where Type = '7B111A3D-2C67-4E21-B9C5-987F0173A66D'
)
DELETE
FROM cte
WHERE sequence > 1
";

        public override void Up()
        {
            Sql(SqlScript);
        }
        
        public override void Down()
        {
        }
    }
}
