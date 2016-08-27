namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixManifestDescriptionTypos : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            Sql(@"
DECLARE @ManifestTypeId uniqueidentifier;

SELECT @ManifestTypeId = Id FROM MtTypes WHERE Alias = 'Manifest Description';

IF (@ManifestTypeId IS NULL)
BEGIN
    RETURN;
END;

UPDATE MtData SET Value4 = 
'{
	""eventNames"" : ""Processed, Failed"",
	""containerDoId"" : ""8431df33-134f-4ba3-bda0-9c0f5257f232"",
	""externalAccountId"" : ""example@example.com"",
	""externalDomainId"" : """",
	""eventPayload"" : [
		{
			""manifestType"": ""Standard UI Controls"",
            ""manifestId"": 6,
            ""manufacturer"": null,
            ""manifestRegistrar"": ""fr8.co/registry"",
            ""id"": ""0eb2f713-55d3-4d12-bb90-d6a32228205d""
		}
	],
	""manufacturer"" : ""Fr8"",
	""Source"" : ""Activity_Name""
}' WHERE Value1 = '7' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	""directUrl"" : ""http://example.com/example.pdf"",
	""fileName"" : ""example.pdf"",
	""fileType"" : ""pdf"",
	""textRepresentation"": """"
}' WHERE Value1 = '10' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	 ""loggingMTkey"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
	 ""item"" : [
		{
			 ""name"" : ""name"",
			 ""primaryCategory"" : ""AuthToken"",
			 ""secondaryCategory"" : ""Created"",
			 ""activity"" : ""AuthToken created"",
			 ""data"" : """",
			 ""isLogged"" : true,
			 ""status"" : """",
			 ""customerId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
			 ""objectId"" : ""0eb2gyt1-g6d3-50lm-bb90-d6a32228205d"",
			 ""lastUpdated"" : ""2016-01-01 10:00:00"",
			 ""createDate"" : ""2016-01-01 10:00:00"",
			 ""discriminator"" : ""Incident"",
			 ""priority"" : 1,
			 ""manufacturer"" : ""Fr8"",
			 ""type"" : ""Terminal""
		}
	 ]
}' WHERE Value1 = '13' AND Type = @ManifestTypeId;

UPDATE MtData SET Value2 = 'DocuSignEnvelopeCM', Value4 = 
'{
	""status"" : ""Received"",
	""createDate"" : ""2016-01-01 10:00:00"",
	""sendDate"" : ""2016-01-01 10:00:00"",
	""deliveredDate"" : ""2016-01-01 10:00:00"",
	""completedDate"" : ""2016-01-01 10:00:00"",
	""envelopeId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
	""externalAccountId"" : ""example@example.com"",
	""name"" : ""Envelope Name"",
	""subject"" : ""Envelope Subject"",
	""ownerName"" : ""Owner Name"",
	""senderName"" : ""Sender Name"",
	""senderEmail"" : ""Sender Email"",
	""shared"" : ""true"",
	""statusChangeDateTime"" : ""2016-01-01 10:00:00""
}' WHERE Value1 = '15' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	""definition"" : {
		""name"" : ""Terminal Name"",
		""label"" : ""Terminal Label"",
		""version"" : ""1"",
		""terminalStatus"" : ""Active"",
		""endpoint"" : ""http://example.com:8080"",
		""description"" : ""Some description"",
		""authenticationType"" : 1
	},
	""activities"" : [
		{
			""id"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
			""name"" : ""Name"",
			""label"" : ""Label"",
			""version"" : ""1"",
			""tags"" : ""Tags"",
			""category"" : ""Forwarders"",
			""type"" : ""Activity"",
			""needsAuthentication"" : true
		}
	]
}' WHERE Value1 = '23' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	""fileList"" : [
		{
			""directUrl"" : ""http://example.com/example.pdf"",
			""fileName"" : ""example.pdf"",
			""fileType"" : ""pdf"",
			""textRepresentation"" : """"
		}
	]
}' WHERE Value1 = '24' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	""activityCallStack"" : {
		""stack"" : [
			{
				""nodeId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
				""nodeName"" : ""Some Name"",
				""activityExecutionPhase"" : 0,
				""currentChildId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
				""localData"" : []
			}			
		],
		""history"" : [
			{
				""description"" : ""Description""
			}
		],
		""currentActivityResponse"" : {
			""type"" : ""Erorr"",
			""body"" : ""Error Message""
		}
	}
}' WHERE Value1 = '27' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	""id"" : ""1"",
	""name"" : ""SomeManifestCM"",
	""version"" : ""1"",
	""sampleJson"" : ""{ Id : 1 }"",
	""description"" : ""Description"",
	""registeredBy"" : ""Fr8""
}' WHERE Value1 = '30' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	""crateDescriptions"" : [
		{
			""manifestId"" : ""1"",
			""manifestType"" : ""SomeManifestCM"",
			""label"" : ""Crate Label"",
			""producedBy"" : ""ActivityName"",
			""availability"" : ""Runtime"",
			""fields"" : [
				{
					""key"" : ""Key"",
					""value"" : ""Value""
                }
			]
		}
	]
}' WHERE Value1 = '32' AND Type = @ManifestTypeId;

UPDATE MtData SET Value4 = 
'{
	""envelopeId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
	""status"" : ""Signed"",
	""subject"" : ""Subject"",
	""statusChangedDateTime"" : ""2016-01-01 10:00:00"",
	""currentRoutingOrderId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
	""recipients"" : [
		{
			""type"" : ""Type"",
			""name"" : ""Mr X"",
			""email"" : ""example@example.com"",
			""recipientId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
			""routingOrderId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
			""status"" : ""Pending""
		}
	],
	""templates"" : [
		{
			""templateId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d"",
			""name"" : ""Some Name"",
			""documentId"" : ""0eb2f713-55d3-4d12-bb90-d6a32228205d""
		}
	],
	""externalAccountId"" : ""example@example.com"",
	""createDate"" : ""2016-01-01 10:00:00"",
	""sentDate"" : ""2016-01-01 10:00:00""
}' WHERE Value1 = '36' AND Type = @ManifestTypeId;");
        }
        
        public override void Down()
        {
        }
    }
}
