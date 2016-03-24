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

        private void Test<TControl>(TControl sourceControl, Action<TControl, TControl> comparer)
            where  TControl : ControlDefinitionDTO, new()
        {
            var target = new ActivityUi(new TControl() {Name = "Member"});

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
    }
}
