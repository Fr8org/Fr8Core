using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Testing.Unit.Fixtures
{
    public partial class FixtureData
    {
        public static string TestTeamplateId = "58521204-58AF-4E65-8A77-4F4B51FEF626";

        public static List<KeyValueDTO> ListFieldMappings
        {
            get
            {
                return  new List<KeyValueDTO>(){
                    new KeyValueDTO()
                    {
                        Key = "Doctor",
                        Value = "[Customer].physician_string"
                    },
                    new KeyValueDTO()
                    {
                        Key="Condition",
                        Value = "[Customer].medical_condition"
                    }
                };
            }
        }
        public static List<KeyValueDTO> ListFieldMappings2
        {
            get
            {
                return  new List<KeyValueDTO>(){
                    new KeyValueDTO()
                    {
                        Key = "Physician",
                        Value = "[Customer].physician_string"
                    },
                    new KeyValueDTO()
                    {
                        Key="Condition",
                        Value = "[Customer].medical_condition"
                    }
                };
            }
        }

        public static string FieldMappings
        {
            get
            {
                return "{\"fields\": [ { \"name\": \"Doctor\", \"value\": \"[Customer].physician_string\" }, { \"name\": \"Condition\", \"value\": \"[Customer].medical_condition\"} ] }";
            }
        }
        public static string FieldMappings2
        {
            get
            {
                return "{\"fields\": [ { \"name\": \"Physicial\", \"value\": \"[Customer].physician_string\" }, { \"name\": \"Condition\", \"value\": \"[Customer].medical_condition\"} ] }";
            }
        }

        public static List<KeyValueDTO> ListFieldMappings3()
        {

            return new List<KeyValueDTO>(){
                    new KeyValueDTO()
                    {
                        Key = "Physician",
                        Value = "Test1"
                    },
                    new KeyValueDTO()
                    {
                        Key="Condition",
                        Value = "Test2"
                    }
                };
        }

        public static List<KeyValueDTO> ListFieldMappings4()
        {

            return new List<KeyValueDTO>(){
                    new KeyValueDTO()
                    {
                        Key = "ID",
                        Value = "10"
                    },
                    new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "20"
                    },
                      new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "40"
                    }

                };
        }

        public static List<KeyValueDTO> ListFieldMappings5()
        {

            return new List<KeyValueDTO>(){
                    new KeyValueDTO()
                    {
                        Key = "ID",
                        Value = "10"
                    },
                    new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "20"
                    },
                      new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "40"
                    },
                       new KeyValueDTO()
                    {
                        Key="ID",
                        Value = "50"
                    }

                };
        }



       

    }
}
