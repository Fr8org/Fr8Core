Terminal Configuration
==========================

If you want to run Terminals, you need to configure them. In general, this means providing their config files with credentials for whatever web service
you're Terminal is going to connect to. For example, here are the configuration settings used by the .NET Slack Terminal:

![](http://content.screencast.com/users/alexed11/folders/Jing/media/277116d9-accd-4339-840d-d92a8c54958c/2016-08-11_1437.png)

A great first step for would-be Terminal builders is to clone the repo, pick a simple Terminal like Slack, put your own credentials in the config file. At this point, you've
got a few options. You can run up the Hub as well, and create a full local environment (great for learning and powerful, but definitely jumping into the deep end).
You can give your Terminal a distinctive name (i.e. "Joe's Test Slack Terminal"), make sure it's got a publicly visible endpoing by using something like ngrok, and register
the Terminal with the development server at dev.fr8.com. Once that's done, you can register a user account on the development server, and you'll see the Activities
from your Terminal alongside those of the other Terminals the Hub knows about. A third option is to use the swagger interface to send some requests
to your Terminal and study the responses.



Terminal configuration settings are split between a "public" set in the standard Web.config of each Hub or Terminal project, and a private set stored in Settings.config.readme, which is in the Config folder of each project. Example: for terminalSlack project
the settings are at Config/terminalSlack/Settings.config.readme.

Make any appropriate changes and save the resulting file as Settings.config in the same location. Restart the Terminal and you'll be in business.

####URL Endpoint
Each Terminal has a setting in its Web.config file that specifies the address of the terminal. It looks like this:
`  <add key="terminalName.TerminalEndpoint" value="http://localhost:8888" />`
We don't store port settings in the repository, so once you open the solution for the first time Visual Studio will assign random port numbers for all web applications.
If you are using IIS express you can either manually change the address in Web section of project properties for each terminal you are about to run. Or you can use Fr8Core\PortSetter utillity application, that will try to do it for you for all the terminals. 

