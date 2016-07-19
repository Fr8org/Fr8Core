# DefaultTerminalController

Base class for Web API controller that are intended to process terminal related requests from the Hub. 

**Namespace**: Fr8.TerminalBase.BaseClasses  
**Assembly**: Fr8TerminalBase.NET


## Remarks

You have to create one controller that is derived from this class when developing terminals using .NET SDK. This class has no Fr8 specific methods to override. You can override available method from ApiControler, but in general you have not to do this. **DefaultTerminalController** performs all necessary work to successfully process terminal related requests from Hub. 

Typical usage:
```C#
	public class TerminalController : DefaultTerminalController
    {
        public TerminalController(IActivityStore activityStore, IHubDiscoveryService hubDiscovery)
            : base(activityStore, hubDiscovery)
        {
        }
    }
```
