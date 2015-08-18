using Core.PluginRegistrations;
using Data.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.PluginRegistrations
{
    [TestFixture]
    [Category("NotifierPluginRegistration_v1")]
    public class NotifierPluginRegistration_v1Test: BaseTest
    {
        private NotifierPluginRegistration_v1 _notifierPluginRegistration_v1;
        #region configuration setting Json
        private const string emailAction = @"{""configurationSettings"":
                                                        [   {""textField"": 
                                                                {""name"": ""Email Address"",
                                                                ""required"":true,""value"":"""",
                                                                ""fieldLabel"":"""",
                                                                }
                                                            },
                                                            {""textField"": 
                                                                {""name"": ""Friendly Name"",
                                                                ""required"":true,""value"":"""",
                                                                ""fieldLabel"":"""",
                                                                }
                                                            },
                                                            {""textField"": 
                                                                {""name"": ""Subject"",
                                                                ""required"":true,""value"":"""",
                                                                ""fieldLabel"":"""",
                                                                }
                                                            },
                                                            {""textArea"": 
                                                                {""name"": ""Body"",
                                                                ""required"":true,""value"":"""",
                                                                ""fieldLabel"":"""",
                                                                }
                                                            },
                                                        ]
                                                    }";

        private const string textMessageAction = @"{""configurationSettings"":
                                                        [   {""textField"": 
                                                                {""name"": ""Phone Number"",
                                                                ""required"":true,""value"":"""",
                                                                ""fieldLabel"":"""",
                                                                }
                                                            },
                                                            {""textArea"": 
                                                                {""name"": ""Message"",
                                                                ""required"":true,""value"":"""",
                                                                ""fieldLabel"":"""",
                                                                }
                                                            },
                                                        ]
                                                    }";
        #endregion

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _notifierPluginRegistration_v1 = new NotifierPluginRegistration_v1();
        }

        [Test]
        public void CanGetAvailableActions()
        {
            Assert.AreEqual(_notifierPluginRegistration_v1.GetAvailableActions().Count, 2);
            Assert.AreEqual(_notifierPluginRegistration_v1.GetAvailableActions()[0].UserLabel, "Send an Email");
            Assert.AreEqual(_notifierPluginRegistration_v1.GetAvailableActions()[1].UserLabel, "Send a Text (SMS) Message");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void CanGetConfigurationSettings()
        {
            ActionDO curActionForEmail = FixtureData.TestAction4();
            ActionDO curActionForMessage = FixtureData.TestAction5();
            _notifierPluginRegistration_v1.GetConfigurationSettings(null);
            Assert.AreEqual(_notifierPluginRegistration_v1.GetConfigurationSettings(curActionForEmail),emailAction);
            Assert.AreEqual(_notifierPluginRegistration_v1.GetConfigurationSettings(curActionForMessage), textMessageAction);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public void GetConfigurationSettings_CheckForAcitonIsNullOrEmpy()
        {
            ActionDO curActionUserLableEmpty = FixtureData.TestAction6();
            _notifierPluginRegistration_v1.GetConfigurationSettings(curActionUserLableEmpty);
        }

        [Test]
        public void GetFieldMappingTargets_IsNull()
        {
            Assert.IsNull(_notifierPluginRegistration_v1.GetFieldMappingTargets(string.Empty,string.Empty));
        }
    }
}
