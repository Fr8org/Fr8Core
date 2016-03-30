using System;
using System.Collections.Generic;
using Data.Control;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using NUnit.Framework;
using UtilitiesTesting;

namespace DockyardTest.Utilities
{
    [TestFixture]
    public class StandardConfigurationControlsCMTests : BaseTest
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public ControlDefinitionDTO Member;

            public ActivityUi(ControlDefinitionDTO member)
            {
                Member = member;
                Controls.Add(member);
            }
        }

        private void Test<TControl>(TControl sourceControl, Action<TControl, TControl> comparer, Func<TControl> targetControlFactory = null)
            where  TControl : ControlDefinitionDTO, new()
        {
            if (targetControlFactory == null)
            {
                targetControlFactory = () => new TControl();
            }

            var targetControl = targetControlFactory();

            targetControl.Name = "Member";

            var target = new ActivityUi(targetControl);

            sourceControl.Name = "Member";

            var source = new StandardConfigurationControlsCM { Controls = { sourceControl } };

            target.ClonePropertiesFrom(source);

            comparer((TControl)target.Member, (TControl)source.Controls[0]);
        }

        private void AssertListItems(List<ListItem> expected, List<ListItem> actual)
        {
            Assert.AreEqual(expected.Count, expected.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Selected, actual[i].Selected);
                Assert.AreEqual(expected[i].Key, actual[i].Key);
                Assert.AreEqual(expected[i].Value, actual[i].Value);
            }
        }

        [Test]
        public void CanMapTextSource()
        {
            Test(new TextSource
            {
                Selected = true,
                ValueSource = "valueSource",
                ListItems = new List<ListItem>
                {
                    new ListItem { Key = "key1", Selected = true, Value = "value1"},
                    new ListItem { Key = "key2", Selected = false, Value = "value2"},
                    new ListItem { Key = "key3", Selected = false, Value = "value3"},
                }
            }, (target, source) =>
            {
                Assert.AreEqual(source.Selected, target.Selected);
                Assert.AreEqual(source.ValueSource, target.ValueSource);
                AssertListItems(source.ListItems, target.ListItems);
            });
        }

        [Test]
        public void CanMapRadioButtonGroup()
        {
            Test(new RadioButtonGroup
            {
                Selected = true,
                Radios = new List<RadioButtonOption>
                {
                    new RadioButtonOption
                    {
                        Name = "option1",
                        Selected = true,
                        Value = "val option",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new TextBox
                            {
                                Name = "Box1",
                                Label = "label 1",
                                Value = "Box 1 value"
                            }  
                        }
                    }
                }
            }, (target, source) =>
            {
                Assert.AreEqual(source.Selected, target.Selected);
                Assert.AreNotEqual(source.Radios[0], target.Radios[0]);
                Assert.AreNotEqual(source.Radios[0].Controls[0], target.Radios[0].Controls[0]);
                Assert.AreEqual(source.Radios[0].Selected, target.Radios[0].Selected);
                Assert.AreEqual(source.Radios[0].Value, target.Radios[0].Value);
                Assert.AreEqual(source.Radios[0].Controls[0].Label, target.Radios[0].Controls[0].Label);
                Assert.AreEqual(source.Radios[0].Controls[0].Value, target.Radios[0].Controls[0].Value);
            },

            () => new RadioButtonGroup
            {
                Radios = new List<RadioButtonOption>
                {
                    new RadioButtonOption
                    {
                        Name = "option1",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new TextBox
                            {
                                Name = "Box1",
                            }
                        }
                    }
                }
            });
        }
    }
}
