using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.PluginRegistrations
{
    public class NotifierPluginRegistration_v1 : BasePluginRegistration
    {
        public const string baseUrl = "Notifier.BaseUrl";
        private const string availableActions = @"[{ ""ActionType"" : ""Mail"" , ""Version"": ""1.0""}]";
        public NotifierPluginRegistration_v1()
            : base(availableActions, baseUrl)
        {
        }

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
        public List<ActionDO> GetAvailableActions()
        {
            List<ActionDO> curActions = new List<ActionDO>();
            curActions.Add(FillAction(1, "Send an Email"));
            curActions.Add(FillAction(2, "Send a Text (SMS) Message"));
            return curActions;
        }

        public string GetConfigurationSettings(ActionDO curAction)
        {
            string resultJson = string.Empty;
            if(curAction == null)
                throw new ArgumentNullException("curAction");

            if (string.IsNullOrEmpty(curAction.UserLabel))
                throw new NullReferenceException("curAction.UserLabel");

            if (curAction.UserLabel.Equals("Send an Email", StringComparison.OrdinalIgnoreCase))
                resultJson = emailAction;
            else if (curAction.UserLabel.Equals("Send a Text (SMS) Message", StringComparison.OrdinalIgnoreCase))
                resultJson = textMessageAction;
            return resultJson;
        }

        public List<string> GetFieldMappingTargets(string curActionName, string ConfigUIData)
        {
            return null;
        }

        private ActionDO FillAction(int id, string userLabel)
        {
            return new ActionDO
            {
                Id = id,
                UserLabel = userLabel,
            };
        }
    }
}
