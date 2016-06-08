using System;
using System.Collections.Generic;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Manifests;

namespace UtilitiesTesting.Fixtures
{
    public static class FixtureData___MultiTenantObjectSubClass
    {
        public static DocuSignEnvelopeCM TestData1()
        {
            return new DocuSignEnvelopeCM()
            {
                EnvelopeId = "1",
                CompletedDate = DateTime.Now,
				DeliveredDate = DateTime.Now.AddDays(1),
                Status = "delivered"
            };
        }

        public static StandardPayloadDataCM TestData2()
        {
            return new StandardPayloadDataCM()
            {
                ObjectType = "ObjectType1",

                PayloadObjects = new List<PayloadObjectDTO>()
                {
                    new PayloadObjectDTO()
                    {
                        PayloadObject = new List<FieldDTO>()
                        {
                            new FieldDTO()
                            {
                                Key = "Key1",
                                Value = "Value1"
                            }
                        }
                    }
                }
            };
        }
    }
}
