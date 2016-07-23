using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Infrastructure.States;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;

using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

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

    class ExplicitTerminalActivityMock : ExplicitTerminalActivity
    {
        public ExplicitTerminalActivityMock(ICrateManager crateManager) 
            : base(crateManager)
        {
        }

        public static ActivityTemplateDTO ActivityTemplate = new ActivityTemplateDTO
        {
            Terminal = new TerminalDTO { Name = "TestTerminal" },
            Name = "ExplicitTerminalActivityMock",
            Version = "1"
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplate;
        public override Task Run()
        {
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }

    class ActivityOverrideCheckMock : TerminalActivity<StandardConfigurationControlsCM>
    {
        public CalledMethod CalledMethods = 0;
        public bool ValidationState = true;

        public ActivityOverrideCheckMock(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public static ActivityTemplateDTO ActivityTemplate = new ActivityTemplateDTO
        {
            Terminal = new TerminalDTO {Name = "TestTerminal"},
            Name = "ActivityOverrideCheckMock"
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplate;

        protected override ConfigurationRequestType GetConfigurationRequestType()
        {
            CheckBasicPropeties();
            return base.GetConfigurationRequestType();
        }

        public override Task Initialize()
        {
            CalledMethods |= CalledMethod.Initialize;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            CalledMethods |= CalledMethod.Configure;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            CalledMethods |= CalledMethod.Run;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            Assert.NotNull(Payload);
            Assert.NotNull(OperationalState);

            return Task.FromResult(0);
        }

        public override Task RunChildActivities()
        {
            CalledMethods |= CalledMethod.ChildActivitiesExecuted;
            CheckBasicPropeties();

            Assert.NotNull(AuthorizationToken);
            Assert.NotNull(Payload);
            Assert.NotNull(OperationalState);

            return Task.FromResult(0);
        }

        public override Task Activate()
        {
            CalledMethods |= CalledMethod.Activate;
            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            CalledMethods |= CalledMethod.Validate;

            CheckBasicPropeties();
            Assert.NotNull(AuthorizationToken);

            if (!ValidationState)
            {
                ValidationManager.SetError("Error");
            }

            return Task.FromResult(0);
        }

        public override Task Deactivate()
        {
            CalledMethods |= CalledMethod.Deactivate;
            CheckBasicPropeties();
            return Task.FromResult(0);
        }

        private void CheckBasicPropeties()
        {
            Assert.NotNull(UiBuilder);
            Assert.NotNull(ActivityPayload);
            Assert.NotNull(Storage);
        }
    }

    class UiSyncDynamicActivityMock : TerminalActivity<UiSyncDynamicActivityMock.ActivityUi>
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


        public UiSyncDynamicActivityMock(ICrateManager crateManager) 
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            OnInitialize?.Invoke(ActivityUI);

            ActivityUI.DynamicTextSources.Add(new TextSource("", "", "ts1"));
            ActivityUI.DynamicTextSources.Add(new TextSource("", "", "ts2"));
            ActivityUI.DynamicTextSources.Add(new TextSource("", "", "ts3"));

            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            OnConfigure?.Invoke(ActivityUI);

            Assert.AreEqual(3, ActivityUI.DynamicTextSources.Count, "Failed to sync dynamic controls list: invalid count");
            Assert.IsTrue(ActivityUI.DynamicTextSources.Any(x => x.Name == "ts1"), "Failed to sync dynamic controls list: ts1 not found");
            Assert.IsTrue(ActivityUI.DynamicTextSources.Any(x => x.Name == "ts2"), "Failed to sync dynamic controls list: ts2 not found");
            Assert.IsTrue(ActivityUI.DynamicTextSources.Any(x => x.Name == "ts3"), "Failed to sync dynamic controls list: ts3 not found");

            Assert.AreEqual("DynamicTextSources_ts1_value", ActivityUI.DynamicTextSources.First(x => x.Name == "ts1").Value, "Failed to sync dynamic controls list: invalid value");
            Assert.AreEqual("DynamicTextSources_ts2_value", ActivityUI.DynamicTextSources.First(x => x.Name == "ts2").Value, "Failed to sync dynamic controls list: invalid value");
            Assert.AreEqual("DynamicTextSources_ts3_value", ActivityUI.DynamicTextSources.First(x => x.Name == "ts3").Value, "Failed to sync dynamic controls list: invalid value");

            return Task.FromResult(0);
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }

        public static ActivityTemplateDTO ActivityTemplate = new ActivityTemplateDTO
        {
            Terminal = new TerminalDTO { Name = "TestTerminal" },
            Name = "UiSyncDynamicActivityMock"
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplate;
    }


    class UiSyncActivityMock : TerminalActivity<UiSyncActivityMock.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplate = new ActivityTemplateDTO
        {
            Terminal = new TerminalDTO { Name = "TestTerminal" },
            Name = "UiSyncActivityMock"
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplate;
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


        public UiSyncActivityMock(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            OnInitialize?.Invoke(ActivityUI);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            OnConfigure?.Invoke(ActivityUI);
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }
       
    }

    class ActivityWithUiBuilder : TerminalActivity<ActivityWithUiBuilder.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplate = new ActivityTemplateDTO
        {
            Terminal = new TerminalDTO { Name = "TestTerminal" },
            Name = "ActivityWithUiBuilder"
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplate;
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public ActivityUi(UiBuilder uiBuilder)
            {
                Assert.IsNotNull(uiBuilder);
            }
        }

        public ActivityWithUiBuilder(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override Task Run()
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
                x.For<IHubCommunicator>().Use(new ExplicitDataHubCommunicator(samplePayload, _crateManager)).Singleton();
            });
            
            FixtureData.AddTestActivityTemplate();
        }

        private ActivityContext CreateActivityContext(params Crate[] crates)
        {
            return new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage(crates)
                },
                AuthorizationToken = new AuthorizationToken()
            };
        }

        [Test]
        public async Task CanInitialize()
        {
            var activity = New<ActivityOverrideCheckMock>();
            await activity.Configure(CreateActivityContext());

            Assert.IsTrue(activity.CalledMethods == CalledMethod.Initialize);
        }

        [Test]
        public async Task CanConfigure()
        {
            var activity = New<ActivityOverrideCheckMock>();
            await activity.Configure(CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM())));
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Configure | CalledMethod.Validate));
        }

        [Test]
        public async Task CanActivate()
        {
            var activity = New<ActivityOverrideCheckMock>();
            var configCrate = Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM());
            var activityContext = CreateActivityContext(configCrate);
            await activity.Activate(activityContext);
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Activate | CalledMethod.Validate));
        }

        [Test]
        public async Task CanDeactivate()
        {
            var activity = New<ActivityOverrideCheckMock>();
            await activity.Deactivate(CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM())));
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Deactivate));
        }
        
        [Test]
        public async Task CanRun()
        {
            var activity = New<ActivityOverrideCheckMock>();
            var executionContext = CreateContainerExecutionContext();
            await activity.Run(CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM())), executionContext);
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Run | CalledMethod.Validate));
        }

        [Test]
        public async Task CanChildActivitiesExecuted()
        {
            var activity = New<ActivityOverrideCheckMock>();
            var executionContext = CreateContainerExecutionContext();
            await activity.RunChildActivities(CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM())), executionContext);
            Assert.IsTrue(activity.CalledMethods == (CalledMethod.ChildActivitiesExecuted | CalledMethod.Validate));
        }

        private ContainerExecutionContext CreateContainerExecutionContext()
        {
            return new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent("", new OperationalStateCM()))
            };
        }

        [Test]
        public async Task ErrorOnRunWithFailedValidation()
        {
            var activity = New<ActivityOverrideCheckMock>();

            activity.ValidationState = false;

            var executionContext = CreateContainerExecutionContext();

            var activityContext = CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM()));

            await activity.Run(activityContext, executionContext);

            Assert.IsTrue(activity.CalledMethods == (CalledMethod.Validate));

            var storage = executionContext.PayloadStorage;
            var opState = storage.CrateContentsOfType<OperationalStateCM>().Single();

            Assert.AreEqual(ActivityResponse.Error.ToString(), opState.CurrentActivityResponse.Type); 
        }

        [Test]
        public async Task CanReturnConfigurationControlsAfterInitialize()
        {
            var activity = New<UiSyncActivityMock>();
            var activityContext = CreateActivityContext();
            await activity.Configure(activityContext);
            var cc = activityContext.ActivityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var refCC = new UiSyncActivityMock.ActivityUi();

            AssertEquals(refCC, cc);
        }

        [Test]
        public async Task ReturnChangedConfigurationControlsAfterConfig()
        {
            var activity = New<UiSyncActivityMock>();
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
            var activityContext = CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, refCC));
            await activity.Configure(activityContext);
            var cc = activityContext.ActivityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            
            AssertEquals(refCC, cc);
        }

        [Test]
        public async Task CanUpdateConfigurationControlsAfterConfig()
        {
            var activity = New<UiSyncActivityMock>();

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

            var activityContext = CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, refCC));
            await activity.Configure(activityContext);
            var cc = activityContext.ActivityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

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
            var activity = New<UiSyncActivityMock>();
            await activity.Configure(CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM())));
        }

        [Test]
        public async Task CanStoreDynamicControls()
        {
            var activity = New<UiSyncDynamicActivityMock>();
            var activityContext = CreateActivityContext();
            await activity.Configure(activityContext);
            var cc = activityContext.ActivityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.AreEqual(5, cc.Controls.Count,  "Failed to sync dynamic controls list: invalid count");
            Assert.AreEqual("DynamicTextSources_ts1", cc.Controls[1].Name, "Failed to sync dynamic controls list: ts1 not found at [1]");
            Assert.AreEqual("DynamicTextSources_ts2", cc.Controls[2].Name, "Failed to sync dynamic controls list: ts2 not found at [2]");
            Assert.AreEqual("DynamicTextSources_ts3", cc.Controls[3].Name, "Failed to sync dynamic controls list: ts3 not found at [3]");
        }


        [Test]
        public async Task CanRestoreDynamicControls()
        {
            var activity = New<UiSyncDynamicActivityMock>();
            var activityContext = CreateActivityContext();
            await activity.Configure(activityContext);
            var cc = activityContext.ActivityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            foreach (var controlDefinitionDto in cc.Controls)
            {
                controlDefinitionDto.Value = controlDefinitionDto.Name + "_value";
            }
            await activity.Configure(CreateActivityContext(Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, cc)));
            //cc = _crateManager.GetStorage(dto).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
        }
    }
}
