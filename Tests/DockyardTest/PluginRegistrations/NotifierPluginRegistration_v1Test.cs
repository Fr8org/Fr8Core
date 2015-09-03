using System;
using System.Collections.Generic;
using System.Linq;
using Core.PluginRegistrations;
using Data.Entities;
using Newtonsoft.Json;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.PluginRegistrations
{
    [TestFixture]
    [Category("NotifierPluginRegistration_v1")]
    public class NotifierPluginRegistration_v1Test : BaseTest
    {
        private NotifierPluginRegistration_v1 _notifierPluginRegistration_v1;
#region configuration setting Json
        private const string emailAction = @"{""fields"":[{""name"":""Email Address"",""required"":true,""value"":"""",""fieldLabel"":"""",""type"":""textboxField"",""selected"":false},{""name"":""Friendly Name"",""required"":true,""value"":"""",""fieldLabel"":"""",""type"":""textboxField"",""selected"":false},{""name"":""Subject"",""required"":true,""value"":"""",""fieldLabel"":"""",""type"":""textboxField"",""selected"":false},{""name"":""Body"",""required"":true,""value"":"""",""fieldLabel"":"""",""type"":""textboxField"",""selected"":false}]}";
        private const string textMessageAction = @"{""fields"":[{""name"":""Phone Number"",""required"":true,""value"":"""",""fieldLabel"":"""",""type"":""textboxField"",""selected"":false},{""name"":""Message"",""required"":true,""value"":"""",""fieldLabel"":"""",""type"":""textboxField"",""selected"":false}]}";
#endregion

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _notifierPluginRegistration_v1 = new NotifierPluginRegistration_v1();
            _notifierPluginRegistration_v1.RegisterActions();
        }

        [Test]
        public void CanGetAvailableActions()
        {
            Assert.AreEqual(_notifierPluginRegistration_v1.AvailableActions.Count(), 2);
            Assert.AreEqual(((List<ActionTemplateDO>)_notifierPluginRegistration_v1.AvailableActions)[0].ActionType, "Send an Email");
            Assert.AreEqual(((List<ActionTemplateDO>)_notifierPluginRegistration_v1.AvailableActions)[1].ActionType, "Send a Text (SMS) Message");
        }

        [Test]
        public void CanGetConfigurationSettings()
        {
            ActionDO curActionForEmail = FixtureData.TestAction4();
            ActionDO curActionForMessage = FixtureData.TestAction5();
            string resultJsonEmail = JsonConvert.SerializeObject(_notifierPluginRegistration_v1.GetConfigurationSettings(curActionForEmail));
            string resultJsonMessage = JsonConvert.SerializeObject(_notifierPluginRegistration_v1.GetConfigurationSettings(curActionForMessage));
            Assert.AreEqual(resultJsonEmail, emailAction);
            Assert.AreEqual(resultJsonMessage, textMessageAction);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void GetConfigurationSettings_CheckForAcitonIsNullOrEmpy()
        {
            ActionDO curActionNameEmpty = FixtureData.TestAction6();
            _notifierPluginRegistration_v1.GetConfigurationSettings(curActionNameEmpty);
        }

        [Test]
        public void GetFieldMappingTargets_IsNull()
        {
            Assert.IsNull(_notifierPluginRegistration_v1.GetFieldMappingTargets(string.Empty, string.Empty));
        }
    }
}