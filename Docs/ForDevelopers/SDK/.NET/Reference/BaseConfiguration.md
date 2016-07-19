# BaseConfiguration

Base class for building Owin startup class for the terminal. When developing new terminal, you must derive your Owin startup class from BaseConfiguration. 

**Namespace**: Fr8.TerminalBase.BaseClasses  
**Assembly**: Fr8TerminalBase.NET


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| ConfigureProject(bool, Action\<ConfigurationExpression>)   | Configures all required stuff to make terminal running correctly. This methods gives you a chance to configure DI container with your services.|
|RegisterActivities()| Method where the terminal should register activities it manages.|
|ConfigureFormatters()|Method where message formatters are configured. |
|GetControllerTypes()|Method that is used to get list of controllers during the self-hosted mode. |

## Remarks

Typical Owin startup class for a terminal:
```C#
	public class Startup : BaseConfiguration
    {
        public Startup()
            : base(TerminalData.TerminalDTO)
        {
        }

        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, MyTerminal.StructureMapConfiguration);
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting();
            }
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<MyTerminalActivity>(MyTerminalActivity.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
```