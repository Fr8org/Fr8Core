using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace Fr8.Testing.Unit.Fixtures
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
                        PayloadObject = new List<KeyValueDTO>()
                        {
                            new KeyValueDTO()
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
