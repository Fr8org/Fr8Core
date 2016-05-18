using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace terminaBaselTests.BaseClasses
{
    [Flags]
    enum CalledMethod
    {
        None = 0,
        Initialize = 0x1,
        Configure = 0x2,
        Run = 0x4,
        ChildActivitiesExecuted = 0x8,
        Activate = 0x10,
        Deactivate = 0x20,
        Validate = 0x40,
    }

    class ActivityOverrideCheckMock : EnhancedTerminalActivity<StandardConfigurationControlsCM>
    {
        public CalledMethod CalledMethods = 0;
        public bool ValidationState = true;

        public ActivityOverrideCheckMock(bool isAuthenticationRequired)
            : base(isAuthenticationRequired)
        {
        }

        protected override StandardConfigurationControlsCM CrateConfigurationControls()
        {
            return new StandardConfigurationControlsCM();
        }

        protected override ConfigurationRequestType GetConfigurationRequestType()
        {
            CheckBasicPropeties();
            return base.GetConfigurationRequestType();
        }

        protected override Task InitializeETA()
        {
            CalledMethods |= CalledMethod.Initialize;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            return Task.FromResult(0);
        }

        protected override Task ConfigureETA()
        {
            CalledMethods |= CalledMethod.Configure;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            CalledMethods |= CalledMethod.Run;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            Assert.NotNull(CurrentPayloadStorage);
            Assert.NotNull(OperationalState);

            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            CalledMethods |= CalledMethod.ChildActivitiesExecuted;
            CheckBasicPropeties();

            Assert.NotNull(AuthorizationToken);
            Assert.NotNull(CurrentPayloadStorage);
            Assert.NotNull(OperationalState);

            return Task.FromResult(0);
        }

        protected override Task Activate()
        {
            CalledMethods |= CalledMethod.Activate;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            return Task.FromResult(0);
        }

        protected override Task Validate(ValidationManager validationManager)
        {
            CalledMethods |= CalledMethod.Validate;

            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);

            if (!ValidationState)
            {
                validationManager.SetError("Error");
            }

            return Task.FromResult(ValidationState);
        }

        protected override Task Deactivate()
        {
            CalledMethods |= CalledMethod.Deactivate;
            CheckBasicPropeties();
            return Task.FromResult(0);
        }

        private void CheckBasicPropeties()
        {
            Assert.NotNull(UiBuilder);
            Assert.NotNull(UpstreamQueryManager);
            Assert.NotNull(CurrentActivity);
            Assert.NotNull(CurrentActivityStorage);
        }
    }

    class UiSyncDynamicActivityMock : EnhancedTerminalActivity<UiSyncDynamicActivityMock.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox TextBox;
            [DynamicControls]
            public List<TextSource> DynamicTextSources = new List<TextSource>();

            public UpstreamCrateChooser UpstreamUpstreamCrateChooser;

            public ActivityUi()
            {
                Add(TextBox = new TextBox
                {
                    Name = "tb1",
                    Value = "tb1_v"
                });
                

                Add(UpstreamUpstreamCrateChooser = new UpstreamCrateChooser
                {
                    Name = "crateChooser"
                });
            }
        }

        public Action<ActivityUi> OnConfigure;
        public Action<ActivityUi> OnInitialize;

        public UiSyncDynamicActivityMock()
            : base(false)
        {
        }

        protected override Task Initialize(CrateSignaller crateSignaller)
        {
            OnInitialize?.Invoke(ConfigurationControls);

            ConfigurationControls.DynamicTextSources.Add(new TextSource("", "", "ts1"));
            ConfigurationControls.DynamicTextSources.Add(new TextSource("", "", "ts2"));
            ConfigurationControls.DynamicTextSources.Add(new TextSource("", "", "ts3"));

            return Task.FromResult(0);
        }

        protected override Task Configure(CrateSignaller crateSignaller, ValidationManager validationManager)
        {
            OnConfigure?.Invoke(ConfigurationControls);

            Assert.AreEqual(3, ConfigurationControls.DynamicTextSources.Count, "Failed to sync dynamic controls list: invalid count");
            Assert.IsTrue(ConfigurationControls.DynamicTextSources.Any(x => x.Name == "ts1"), "Failed to sync dynamic controls list: ts1 not found");
            Assert.IsTrue(ConfigurationControls.DynamicTextSources.Any(x => x.Name == "ts2"), "Failed to sync dynamic controls list: ts2 not found");
            Assert.IsTrue(ConfigurationControls.DynamicTextSources.Any(x => x.Name == "ts3"), "Failed to sync dynamic controls list: ts3 not found");

            Assert.AreEqual("DynamicTextSources_ts1_value", ConfigurationControls.DynamicTextSources.First(x => x.Name == "ts1").Value, "Failed to sync dynamic controls list: invalid value");
            Assert.AreEqual("DynamicTextSources_ts2_value", ConfigurationControls.DynamicTextSources.First(x => x.Name == "ts2").Value, "Failed to sync dynamic controls list: invalid value");
            Assert.AreEqual("DynamicTextSources_ts3_value", ConfigurationControls.DynamicTextSources.First(x => x.Name == "ts3").Value, "Failed to sync dynamic controls list: invalid value");

            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            return Task.FromResult(0);
        }
    }


    class UiSyncActivityMock : EnhancedTerminalActivity<UiSyncActivityMock.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource TextSource;
            public TextBox TextBox;
            public DropDownList DropDownList;
            public RadioButtonGroup Group;
            public DropDownList NestedDropDown;
            public UpstreamCrateChooser UpstreamUpstreamCrateChooser;

            public ActivityUi()
            {
                Add(TextBox = new TextBox
                {
                    Name = "tb1",
                    Value = "tb1_v"
                });
                
                Add(new TextBox
                {
                    Name = "textBox",
                    Value = "textBox_value"
                });

                Add(UpstreamUpstreamCrateChooser = new UpstreamCrateChooser
                {
                    Name = "crateChooser"
                });
            }
        }

        public Action<ActivityUi> OnConfigure;
        public Action<ActivityUi> OnInitialize;

        public UiSyncActivityMock() 
            : base(false)
        {
        }

        protected override Task Initialize(CrateSignaller crateSignaller)
        {
            OnInitialize?.Invoke(ConfigurationControls);
            return Task.FromResult(0);
        }

        protected override Task Configure(CrateSignaller crateSignaller, ValidationManager validationManager)
        {
            OnConfigure?.Invoke(ConfigurationControls);
            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            return Task.FromResult(0);
        }
    }

    class ActivityWithUiBuilder : EnhancedTerminalActivity<ActivityWithUiBuilder.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public ActivityUi(UiBuilder uiBuilder)
            {
                Assert.IsNotNull(uiBuilder);
            }
        }

        public ActivityWithUiBuilder()
            :base(false)
        {
        }

        protected override Task Initialize(CrateSignaller crateSignaller)
        {
            return Task.FromResult(0);
        }

        protected override Task Configure(CrateSignaller crateSignaller, ValidationManager validationManager)
        {
            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            return Task.FromResult(0);
        }
    }

    [TestFixture]
    [Category("EnhancedTerminalActivityTests")]
    public class EnhancedTerminalActivityTests : BaseTest
    {
        private ICrateManager _crateManager;

        private void AssertEquals(StandardConfigurationControlsCM expected, StandardConfigurationControlsCM actual)
        {
            expected = new StandardConfigurationControlsCM(expected.Controls);
            actual = new StandardConfigurationControlsCM(actual.Controls);

            var crate = Crate.FromContent(string.Empty, expected);

            var expectedDto = CrateStorageSerializer.Default.ConvertToDto(crate);

            crate.Put(actual);
            var actualdDto = CrateStorageSerializer.Default.ConvertToDto(crate);

            Assert.AreEqual(JsonConvert.SerializeObject(expectedDto), JsonConvert.SerializeObject(actualdDto));
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            TerminalBootstrapper.ConfigureTest();

            _crateManager = ObjectFactory.GetInstance<ICrateManager>();

            string samplePayload = _crateManager.CrateStorageAsStr(new CrateStorage(Crate.FromContent("ExplicitData_PayloadCrate", new OperationalStateCM())));

            ObjectFactory.Configure(x =>
            {
                x.For<IRestfulServiceClient>().Use<RestfulServiceClient>().SelectConstructor(() => new RestfulServiceClient());
                x.For<IHubCommunicator>().Use(new ExplicitDataHubCommunicator(samplePayload)).Singleton();
            });
            
            FixtureData.AddTestActivityTemplate();
        }

        private ActivityDO CreateActivity(params Crate[] crates)
        {
            return new ActivityDO
            {
                CrateStorage = _crateManager.CrateStorageAsStr(new CrateStorage(crates))
            };
        }

        [Test]
        public async Task CanInitialize()
        {
            var activity = new ActivityOverrideCheckMock(false);
            await activity.Configure(CreateActivity(), new AuthorizationTokenDO());

            Assert.IsTrue(activity.CalledMethods == CalledMethod.Initialize);
        }

        [Test]
        public async Task CanConfigure()
        {
            var activity = new ActivityOverrideCheckMock(false);
            await activity.Configure(CreateActivity(Crate.FromContent("crate", new StandardConfigurationControlsCM())), new AuthorizationTokenDO());
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Configure | CalledMethod.Validate));
        }

        [Test]
        public async Task CanActivate()
        {
            var activity = new ActivityOverrideCheckMock(false);
            await activity.Activate(CreateActivity(Crate.FromContent("crate", new StandardConfigurationControlsCM())), new AuthorizationTokenDO());
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Activate | CalledMethod.Validate));
        }

        [Test]
        public async Task CanDeactivate()
        {
            var activity = new ActivityOverrideCheckMock(false);
            await activity.Deactivate(CreateActivity(Crate.FromContent("crate", new StandardConfigurationControlsCM())));
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Deactivate));
        }
        
        [Test]
        public async Task CanRun()
        {
            var activity = new ActivityOverrideCheckMock(false);
            await ObjectFactory.GetInstance<IHubCommunicator>().Configure("testTerminal");
            await activity.Run(CreateActivity(Crate.FromContent("crate", new StandardConfigurationControlsCM())), Guid.Empty, new AuthorizationTokenDO());
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Run | CalledMethod.Validate));
        }

        [Test]
        public async Task CanChildActivitiesExecuted()
        {
            var activity = new ActivityOverrideCheckMock(false);
            await ObjectFactory.GetInstance<IHubCommunicator>().Configure("testTerminal");
            await activity.ExecuteChildActivities(CreateActivity(Crate.FromContent("crate", new StandardConfigurationControlsCM())), Guid.Empty, new AuthorizationTokenDO());
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.ChildActivitiesExecuted | CalledMethod.Validate));
        }

        [Test]
        public async Task ErrorOnRunWithFailedValidation()
        {
            var activity = new ActivityOverrideCheckMock(false)
            {
                ValidationState = false
            };
            
            await ObjectFactory.GetInstance<IHubCommunicator>().Configure("testTerminal");

            var payload = await activity.Run(CreateActivity(Crate.FromContent("crate", new StandardConfigurationControlsCM())), Guid.Empty, new AuthorizationTokenDO());

            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Validate));

            var storage = _crateManager.GetStorage(payload);
            var opState = storage.CrateContentsOfType<OperationalStateCM>().Single();

            Assert.AreEqual(ActivityResponse.Error.ToString(), opState.CurrentActivityResponse.Type); 
        }

        [Test]
        public async Task CanReturnConfigurationControlsAfterInitialize()
        {
            var activity = new UiSyncActivityMock();
            var dto = await activity.Configure(CreateActivity(), new AuthorizationTokenDO());
            var cc = _crateManager.GetStorage(dto).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var refCC = new UiSyncActivityMock.ActivityUi();

            AssertEquals(refCC, cc);
        }

        [Test]
        public async Task ReturnChangedConfigurationControlsAfterConfig()
        {
            var activity = new UiSyncActivityMock();
            var refCC = new UiSyncActivityMock.ActivityUi();

            refCC.TextBox.Value = "value";
            refCC.FindByNameNested<TextBox>("textBox").Value = "some other value";
            refCC.UpstreamUpstreamCrateChooser.MultiSelection = true;
            refCC.UpstreamUpstreamCrateChooser.SelectedCrates.Add(new CrateDetails()
            {
                Label = new DropDownList()
                {
                    selectedKey = "sk1",
                    Value = "val1",
                    ListItems = {new ListItem() {Key = "sk1", Selected = true, Value = "sk1"},
                                 new ListItem() {Key = "sk2", Selected = false, Value = "sk2"} }
                },
                ManifestType = new DropDownList()
                {
                    selectedKey = "sk2",
                    Value = "val2",
                    ListItems = {new ListItem() {Key = "sk1", Selected = true, Value = "sk1"},
                                 new ListItem() {Key = "sk2", Selected = false, Value = "sk2"} }
                }
            });

            var dto = await activity.Configure(CreateActivity(Crate.FromContent("", refCC)), new AuthorizationTokenDO());
            var cc = _crateManager.GetStorage(dto).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            
            AssertEquals(refCC, cc);
        }

        [Test]
        public async Task CanUpdateConfigurationControlsAfterConfig()
        {
            var activity = new UiSyncActivityMock();

            activity.OnConfigure = x =>
            {
                x.TextBox.Value = "123123123";

                x.UpstreamUpstreamCrateChooser.SelectedCrates.Add(new CrateDetails()
                {
                    Label = new DropDownList()
                    {
                        selectedKey = "sk1",
                        Value = "val1",
                        ListItems = {new ListItem() {Key = "sk1", Selected = true, Value = "sk1"},
                                 new ListItem() {Key = "sk2", Selected = false, Value = "sk2"} }
                    },
                    ManifestType = new DropDownList()
                    {
                        selectedKey = "sk2",
                        Value = "val2",
                        ListItems = {new ListItem() {Key = "sk1", Selected = true, Value = "sk1"},
                                 new ListItem() {Key = "sk2", Selected = false, Value = "sk2"} }
                    }
                });
            };

            var refCC = new UiSyncActivityMock.ActivityUi();

            var dto = await activity.Configure(CreateActivity(Crate.FromContent("", refCC)), new AuthorizationTokenDO());
            var cc = _crateManager.GetStorage(dto).CrateContentsOfType<StandardConfigurationControlsCM>().Single();

            refCC.TextBox.Value = "123123123";
            refCC.UpstreamUpstreamCrateChooser.SelectedCrates.Add(new CrateDetails()
            {
                Label = new DropDownList()
                {
                    selectedKey = "sk1",
                    Value = "val1",
                    ListItems = {new ListItem() {Key = "sk1", Selected = true, Value = "sk1"},
                                 new ListItem() {Key = "sk2", Selected = false, Value = "sk2"} }
                },
                ManifestType = new DropDownList()
                {
                    selectedKey = "sk2",
                    Value = "val2",
                    ListItems = {new ListItem() {Key = "sk1", Selected = true, Value = "sk1"},
                                 new ListItem() {Key = "sk2", Selected = false, Value = "sk2"} }
                }
            });

            AssertEquals(refCC, cc);
        }

        [Test]
        public async Task CanUseUiBuilder()
        {
            var activity = new ActivityWithUiBuilder();
            await activity.Configure(CreateActivity(Crate.FromContent("crate", new StandardConfigurationControlsCM())), new AuthorizationTokenDO());
        }

        [Test]
        public async Task CanStoreDynamicControls()
        {
            var activity = new UiSyncDynamicActivityMock();
            var dto = await activity.Configure(CreateActivity(), new AuthorizationTokenDO());
            var cc = _crateManager.GetStorage(dto).CrateContentsOfType<StandardConfigurationControlsCM>().Single();

            Assert.AreEqual(5, cc.Controls.Count,  "Failed to sync dynamic controls list: invalid count");
            Assert.AreEqual("DynamicTextSources_ts1", cc.Controls[1].Name, "Failed to sync dynamic controls list: ts1 not found at [1]");
            Assert.AreEqual("DynamicTextSources_ts2", cc.Controls[2].Name, "Failed to sync dynamic controls list: ts2 not found at [2]");
            Assert.AreEqual("DynamicTextSources_ts3", cc.Controls[3].Name, "Failed to sync dynamic controls list: ts3 not found at [3]");
        }


        [Test]
        public async Task CanRestoreDynamicControls()
        {
            var activity = new UiSyncDynamicActivityMock();
            var dto = await activity.Configure(CreateActivity(), new AuthorizationTokenDO());
            var cc = _crateManager.GetStorage(dto).CrateContentsOfType<StandardConfigurationControlsCM>().Single();

            foreach (var controlDefinitionDto in cc.Controls)
            {
                controlDefinitionDto.Value = controlDefinitionDto.Name + "_value";
            }

            await activity.Configure(CreateActivity(Crate.FromContent("cc", cc)), new AuthorizationTokenDO());
            //cc = _crateManager.GetStorage(dto).CrateContentsOfType<StandardConfigurationControlsCM>().Single();

            

        }
    }
}
