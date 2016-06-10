using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Testing.Unit.Fixtures
{
    public partial class FixtureData
    {
        public static string TestTeamplateId = "58521204-58AF-4E65-8A77-4F4B51FEF626";

        public static List<FieldDTO> ListFieldMappings
        {
            get
            {
                return  new List<FieldDTO>(){
                    new FieldDTO()
                    {
                        Key = "Doctor",
                        Value = "[Customer].physician_string"
                    },
                    new FieldDTO()
                    {
                        Key="Condition",
                        Value = "[Customer].medical_condition"
                    }
                };
            }
        }
        public static List<FieldDTO> ListFieldMappings2
        {
            get
            {
                return  new List<FieldDTO>(){
                    new FieldDTO()
                    {
                        Key = "Physician",
                        Value = "[Customer].physician_string"
                    },
                    new FieldDTO()
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

        public static List<FieldDTO> ListFieldMappings3()
        {

            return new List<FieldDTO>(){
                    new FieldDTO()
                    {
                        Key = "Physician",
                        Value = "Test1"
                    },
                    new FieldDTO()
                    {
                        Key="Condition",
                        Value = "Test2"
                    }
                };
        }

        public static List<FieldDTO> ListFieldMappings4()
        {

            return new List<FieldDTO>(){
                    new FieldDTO()
                    {
                        Key = "ID",
                        Value = "10"
                    },
                    new FieldDTO()
                    {
                        Key="ID",
                        Value = "20"
                    },
                      new FieldDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new FieldDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new FieldDTO()
                    {
                        Key="ID",
                        Value = "40"
                    }

                };
        }

        public static List<FieldDTO> ListFieldMappings5()
        {

            return new List<FieldDTO>(){
                    new FieldDTO()
                    {
                        Key = "ID",
                        Value = "10"
                    },
                    new FieldDTO()
                    {
                        Key="ID",
                        Value = "20"
                    },
                      new FieldDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new FieldDTO()
                    {
                        Key="ID",
                        Value = "30"
                    },
                       new FieldDTO()
                    {
                        Key="ID",
                        Value = "40"
                    },
                       new FieldDTO()
                    {
                        Key="ID",
                        Value = "50"
                    }

                };
        }



       

    }
}
