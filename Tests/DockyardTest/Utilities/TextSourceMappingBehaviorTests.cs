using System.Linq;
using NUnit.Framework;
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using TerminalBase.Infrastructure.Behaviors;
using UtilitiesTesting;

namespace DockyardTest.Utilities
{
    [TestFixture]
    public class TextSourceMappingBehaviorTests : BaseTest
    {
        private string[] CreateFieldIds()
        {
            return new[] { "Field 1", "Field 2", "Field 3" };
        }

        private TextSourceMappingBehavior CreateCrateStorageWithBehavior(string[] fieldIds)
        {
            var crateStorage = new CrateStorage();

            var behavior = new TextSourceMappingBehavior(crateStorage, "Test");
            behavior.Append(fieldIds, "Test upstream data");

            return behavior;
        }

        [Test]
        public void TextSourceMappingBehavior_Append()
        {
            var fieldIds = CreateFieldIds();
            var behavior = CreateCrateStorageWithBehavior(fieldIds);

            var configControls = behavior.CrateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .FirstOrDefault();

            Assert.NotNull(configControls);
            Assert.NotNull(configControls.Controls);
            Assert.AreEqual(fieldIds.Length, configControls.Controls.Count);
            
            for (var i = 0; i < fieldIds.Length; ++i)
            {
                var textSource = configControls.Controls[i] as TextSource;
                Assert.NotNull(textSource);

                Assert.AreEqual(TextSourceMappingBehavior.BehaviorPrefix + fieldIds[i], textSource.Name);
            }
        }

        [Test]
        public void TextSourceMappingBehavior_Clear()
        {
            var fieldIds = CreateFieldIds();
            var behavior = CreateCrateStorageWithBehavior(fieldIds);

            var configControls = behavior.CrateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .FirstOrDefault();

            Assert.AreEqual(fieldIds.Length, configControls.Controls.Count);

            behavior.Clear();

            Assert.AreEqual(0, configControls.Controls.Count);
        }

        [Test]
        public void TextSourceMappingBehavior_GetValues()
        {
            var fieldIds = CreateFieldIds();
            var behavior = CreateCrateStorageWithBehavior(fieldIds);

            var configControls = behavior.CrateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .FirstOrDefault();

            var n = 0;
            foreach (var textSource in configControls.Controls.OfType<TextSource>())
            {
                textSource.ValueSource = "specific";
                textSource.TextValue = "TestValue " + (n + 1).ToString();

                ++n;
            }

            var data = behavior.GetValues();

            Assert.NotNull(data);
            Assert.AreEqual(fieldIds.Length, data.Count);
            
            for (var i = 0; i < fieldIds.Length; ++i)
            {
                Assert.True(data.ContainsKey(fieldIds[i]));
                Assert.AreEqual("TestValue " + (i + 1).ToString(), data[fieldIds[i]]);
            }
        }
    }
}
