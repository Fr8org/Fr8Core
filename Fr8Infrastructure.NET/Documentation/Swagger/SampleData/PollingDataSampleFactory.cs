using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PollingDataSampleFactory : ISwaggerSampleFactory<PollingDataDTO>
    {
        public PollingDataDTO GetSampleData()
        {
            return new PollingDataDTO
            {
                ExternalAccountId = "X123456",
                UserId = "EFA26398-BA95-4E57-9133-16E481FE80E5",
                AuthToken = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
                Fr8AccountId = "0902D5A3-B578-4F88-81DB-1C41869866C7",
                AdditionToJobId = "5ECB3A54-BF41-4C82-B414-E729C43B649F",
                AdditionalConfigAttributes = "attributes",
                JobId = "CD19A3DF-2495-4C77-A305-2D2D77C70447",
                Payload = "{ \"name\" : \"value\" }",
                PollingIntervalInMinutes = "5",
                Result = true,
                RetryCounter = 3,
                TriggerImmediately = true
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}