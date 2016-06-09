using System;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using NUnit.Framework;
using Fr8.Testing.Unit;

namespace HubTests
{
    [TestFixture]
    [Category("ManifestDiscovery")]
    public class ManifestDiscoveryTests : BaseTest
    {
        public class CustomManifest : Manifest
        {
            public CustomManifest()
                : base(7777771, "CustomManifest")
            {
            }
        }

        [CrateManifestType(777772, "CustomManiefstWithAttributes")]
        public class CustomManifestWithAttributes
        {
        }

        public class InvalidManifest
        {
        }

        [Test]
        public void TryResolveTypeForKnownType_ReturnsTrue()
        {
            var manifest = new FieldDescriptionsCM();
            Type dummy;

            Assert.IsTrue(ManifestDiscovery.Default.TryResolveType(manifest.ManifestType, out dummy));
        }

        [Test]
        public void TryResolveTypeForKnownType_ReturnsCorrectType()
        {
            var manifest = new FieldDescriptionsCM();
            Type dummy;

            ManifestDiscovery.Default.TryResolveType(manifest.ManifestType, out dummy);
            Assert.AreEqual(manifest.GetType(), dummy);
        }

        [Test]
        public void TryResolveTypeForUnknownType_ReturnsFalse()
        {
            Type dummy;
            Assert.IsFalse(ManifestDiscovery.Default.TryResolveType(new CrateManifestType(null, 777777777), out dummy));
        }

        [Test]
        public void RegisterManifest_DerivedFromManifest_Succeed()
        {
            Type dummy;

            ManifestDiscovery.Default.RegisterManifest(typeof(CustomManifest));

            ManifestDiscovery.Default.TryResolveType(new CrateManifestType(null, 7777771), out dummy);

            Assert.AreEqual(dummy, typeof(CustomManifest));
        }

        [Test]
        public void RegisterManifest_DecoratedWithAttributes_Succeed()
        {
            Type dummy;

            ManifestDiscovery.Default.RegisterManifest(typeof(CustomManifestWithAttributes));

            ManifestDiscovery.Default.TryResolveType(new CrateManifestType(null, 777772), out dummy);

            Assert.AreEqual(dummy, typeof(CustomManifestWithAttributes));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterManifest_InvalidManifest_Fails()
        {
            ManifestDiscovery.Default.RegisterManifest(typeof(InvalidManifest));
        }
    }
}
