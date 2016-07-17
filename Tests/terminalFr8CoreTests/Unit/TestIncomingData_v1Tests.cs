﻿using System;
using System.Collections.Generic;
using System.Linq;
﻿using Fr8.Infrastructure.Data.DataTransferObjects;
﻿using Fr8.TerminalBase.Infrastructure;
﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using terminalFr8Core.Actions;
﻿using terminalFr8Core.Activities;

namespace HubTests.Unit
{
    [TestFixture]
    [Category("Test_Incoming_Data_v1")]
    public class TestIncomingData_v1Tests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
        }

        [Test]
        public void Evaluate_CriteriaIsNull_ExpectedArgumentNullException()
        {
            //Arrange
            var curListFieldMappings = FixtureData.ListFieldMappings;


            object[] parameters = new object[] { null, FixtureData.TestContainer_Id_1(), curListFieldMappings };

            //Act
            var result = Assert.Throws<ArgumentNullException>(() => Invoke<Test_Incoming_Data_v1>("Evaluate", parameters));

            //Assert
            Assert.AreEqual("criteria", result.ParamName);
        }

        [Test]
        public void Evaluate_CriteriaIsEmtpy_ExpectedArgumentException()
        {
            //Arrange
            var curListFieldMappings = FixtureData.ListFieldMappings;

            object[] parameters = new object[] { string.Empty, FixtureData.TestContainer_Id_1(), curListFieldMappings };

            //Act
            var result = Assert.Throws<ArgumentException>(() => Invoke<Test_Incoming_Data_v1>("Evaluate", parameters));

            //Assert
            Assert.AreEqual("criteria", result.ParamName);
        }

        [Test]
        [ExpectedException(typeof(JsonReaderException))]
        public void Evaluate_CriteriaIsInvalidJSON_ExpectedJsonReaderException()
        {
            //Arrange
            var curListFieldMappings = FixtureData.ListFieldMappings;

            object[] parameters = new object[] { "THIS_IS_NOT_CORRECT_JSON_DATA", FixtureData.TestContainer_Id_1(), curListFieldMappings };

            //Act
            Invoke<Test_Incoming_Data_v1>("Evaluate", parameters);
        }

  
        [Test]
        public void Filter_CriteriaIsNull_ExpectedArgumentNullException()
        {
            //Arrange
            var envelopeDataList = FixtureData.ListFieldMappings.AsQueryable();
            object[] parameters = new object[] { null, FixtureData.TestContainer_Id_1(), envelopeDataList };

            //Act
            var ex = Assert.Throws<ArgumentNullException>(() => Invoke<Test_Incoming_Data_v1>("Filter", parameters));

            //Assert
            Assert.AreEqual("criteria", ex.ParamName);
        }

        [Test]
        public void Filter_CriteriaIsEmpty_ExpectedArgumentException()
        {
            //Arrange
            var envelopeDataList = FixtureData.ListFieldMappings.AsQueryable();
            object[] parameters = new object[] { string.Empty, FixtureData.TestContainer_Id_1(), envelopeDataList };

            //Act
            var ex = Assert.Throws<ArgumentException>(() => Invoke<Test_Incoming_Data_v1>("Filter", parameters));

            //Assert
            Assert.AreEqual("criteria", ex.ParamName);
        }


        [Test]
        [ExpectedException(typeof(JsonReaderException))]
        public void Filter_CriteriaIsInvalidJSON_ExpectedJsonReaderException()
        {
            //Arrange
            var curListFieldMappings = FixtureData.ListFieldMappings.AsQueryable();

            object[] parameters = new object[] { "THIS_IS_NOT_CORRECT_JSON_DATA", FixtureData.TestContainer_Id_1(), curListFieldMappings };

            //Act
            Invoke<Test_Incoming_Data_v1>("Filter", parameters);

        }


        ///* One Criteria Equal Operation for string*/
        [Test]
        public void Filter_OneCriteriaEqualOpString_ExpectedEmtpyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings3().AsQueryable();
            string conditionValue = Guid.NewGuid().ToString();
            var criteria = MakeCondition("Physician", "eq", conditionValue);
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);


            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "Physician" && x.Value == conditionValue);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected 0 entries in filtred collection");
        }


        [Test]
        public void Filter_OneCriteriaEqualOpString_ExpectedOneFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings3().AsQueryable();
            var criteria = MakeCondition("Physician", "eq", "Test1");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "Physician" && x.Value == "Test1");
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 1 EnvelopeDataDTO");
        }

        [Test]
        public void Filter_OneCriteriaEqualOpInt_ExpectedOneFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings4().AsQueryable();
            var criteria = MakeCondition("ID", "eq", "10");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && x.Value == "10");
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 1 EnvelopeDataDTO");
        }

        [Test]
        public void Filter_OneCriteriaEqualOpInt_ExpectedTwoFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings4().AsQueryable();
            var criteria = MakeCondition("ID", "eq", "30");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && x.Value == "30");
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 2 EnvelopeDataDTO");
        }

        ///* One Criteria GreaterThan Operation for int*/
        [Test]
        public void Filter_OneCriteriaGreaterThanOpInt_ExpectedEmtpyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings4().AsQueryable();
            var criteria = MakeCondition("ID", "gt", "3000");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) > 3000);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected 0 entries in filtred collection");

        }

        [Test]
        public void Filter_OneCriteriaGreaterThanOpInt_ExpectedOneFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings4().AsQueryable();
            var criteria = MakeCondition("ID", "gt", "30");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) > 30);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 1 EnvelopeDataDTO");
        }

        [Test]
        public void Filter_OneCriteriaGreaterThanOpInt_ExpectedTwoFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "gt", "30");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) > 30);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 2 EnvelopeDataDTO");
        }


        [Test]
        public void Filter_OneCriteriaGreaterThanOpInt_ExpectedManyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "gt", "0");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) > 0);
            Assert.AreEqual(expected.Count(), filtred.Count(), string.Format("Expected only {0} EnvelopeDataDTO", envelopData.Count()));

        }

        ///* One Criteria GreaterThanOrEqual Operation for int*/
        [Test]
        public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedEmtpyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "gte", "3000");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) >= 3000);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected 0 entries in filtred collection");
        }


        [Test]
        public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedOneFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "gte", "50");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) >= 50);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 1 EnvelopeDataDTO");

        }
        [Test]
        public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedTwoFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "gte", "40");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) >= 40);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 2 EnvelopeDataDTO");
        }
        [Test]
        public void Filter_OneCriteriaGreaterThanOrEqualOpInt_ExpectedManyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "gte", "0");

            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) >= 0);
            Assert.AreEqual(expected.Count(), filtred.Count(), string.Format("Expected only {0} EnvelopeDataDTO", envelopData.Count()));
        }

        ///* One Criteria LessThan Operation for int*/
        [Test]
        public void Filter_OneCriteriaLessThanOpInt_ExpectedEmtpyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lt", "10");

            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) < 10);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected 0 entries in filtred collection");
        }

        [Test]
        public void Filter_OneCriteriaLessThanOpInt_ExpectedOneFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lt", "20");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) < 20);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 1 EnvelopeDataDTO");
        }

        [Test]
        public void Filter_OneCriteriaLessThanOpInt_ExpectedTwoFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lt", "30");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) < 30);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 2 EnvelopeDataDTO");
        }

        [Test]
        public void Filter_OneCriteriaLessThanOpInt_ExpectedManyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lt", "3000");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) < 3000);
            Assert.AreEqual(expected.Count(), filtred.Count(), string.Format("Expected only {0} EnvelopeDataDTO", envelopData.Count()));

        }

        ///* One Criteria LessThanOrEqual Operation for int*/
        [Test]
        public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedEmtpyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lte", "0");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) <= 0);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected 0 entries in filtred collection");
        }

        [Test]
        public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedOneFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lte", "10");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) <= 10);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 1 EnvelopeDataDTO");
        }

        [Test]
        public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedTwoFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lte", "20");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) <= 20);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected only 2 EnvelopeDataDTO");
        }

        [Test]
        public void Filter_OneCriteriaLessThanOrEqualOpInt_ExpectedManyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();
            var criteria = MakeCondition("ID", "lte", "3000");
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) <= 3000);
            Assert.AreEqual(expected.Count(), filtred.Count(), string.Format("Expected only {0} EnvelopeDataDTO", envelopData.Count()));
        }

        ///* Two Criteria EqualOp And EqualOp String*/
        [Test]
        public void Filter_TwoCriteriaEqualOpAndEqualOpString_ExpectedEmptyFiltredEnvelope1()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();

            List<FilterConditionDTO> conditons = new List<FilterConditionDTO>();
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "eq", Value = "2000" });
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "eq", Value = "2500" });

            var criteria = MakeCondition(conditons);
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            //Assert
            var expected = envelopData.ToList().Where(x => x.Key == "ID" && Convert.ToInt32(x.Value) == 2000 && Convert.ToInt32(x.Value) == 2500);
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected 0 entries in filtred collection");
        }

        ///* Three Criteria GreaterThan Int And LessThanOrEquals Int*/
        [Test]
        public void Filter_ThreeCriteriaGreaterThanOpIntAndLessThanOrEqualsOpIntAndEqualsOpString_ExpectedEmptyFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();

            List<FilterConditionDTO> conditons = new List<FilterConditionDTO>();
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "gt", Value = "10" });
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "lte", Value = "40" });
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "eq", Value = "400" });


            var criteria = MakeCondition(conditons);
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            var expected = envelopData.ToList().Where(x => Convert.ToInt32(x.Value) > 10 && Convert.ToInt32(x.Value) <= 40 && Convert.ToInt32(x.Value) == 400);

            //Assert
            Assert.AreEqual(expected.Count(), filtred.Count(), "Expected 0 entries in filtred collection");
        }

        [Test]
        public void Filter_ThreeCriteriaGreaterThanOpIntAndLessThanOrEqualsOpIntAndEqualsOpString_ExpectedNFiltredEnvelope()
        {
            //Arrange
            var envelopData = FixtureData.ListFieldMappings5().AsQueryable();

            List<FilterConditionDTO> conditons = new List<FilterConditionDTO>();
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "gt", Value = "10" });
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "lte", Value = "40" });
            conditons.Add(new FilterConditionDTO() { Field = "ID", Operator = "eq", Value = "30" });


            var criteria = MakeCondition(conditons);
            object[] parameters = new object[] { criteria, FixtureData.TestContainer_Id_1(), envelopData };

            //Act
            var filtred = (IQueryable<KeyValueDTO>)Invoke<Test_Incoming_Data_v1>("Filter", parameters);

            var expected = envelopData.ToList().Where(x => Convert.ToInt32(x.Value) > 10 && Convert.ToInt32(x.Value) <= 40 && Convert.ToInt32(x.Value) == 30);

            //Assert
            Assert.AreEqual(expected.Count(), filtred.Count(), string.Format("Expected {0} entries in filtred collection", expected.Count()));
        }


        private static string MakeCondition(string field, string op, string value)
        {
            FilterDataDTO curFilterDataDTO = new FilterDataDTO();
            curFilterDataDTO.ExecutionType = FilterExecutionType.WithFilter;
            curFilterDataDTO.Conditions = new List<FilterConditionDTO>();
            curFilterDataDTO.Conditions.Add(new FilterConditionDTO()
            {
                Field = field,
                Operator = op,
                Value = value

            });

            // return JsonConvert.SerializeObject(curFilterDataDTO);
            return JsonConvert.SerializeObject(curFilterDataDTO, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        private static string MakeCondition(List<FilterConditionDTO> conditions)
        {
            FilterDataDTO curFilterDataDTO = new FilterDataDTO();
            curFilterDataDTO.ExecutionType = FilterExecutionType.WithFilter;
            curFilterDataDTO.Conditions = new List<FilterConditionDTO>();
            curFilterDataDTO.Conditions.AddRange(conditions);

            return JsonConvert.SerializeObject(curFilterDataDTO, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

    }
}
