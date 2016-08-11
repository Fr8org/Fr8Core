Building and Running Fr8
========================


1. Open the Fr8 solution in Visual Studio.

2. Decide which Terminals you want to run with and verify that your solution is set to open [Multiple Startup Projects](/Docs/ForDevelopers/DevelopmentGuides/Terminals/dotNet/MultipleStartupProjects.md)

*Note*: most Terminals are associated intimately with a web service like Salesforce.com or SendGrid. To get them to work locally, you're going to need to obtain a developer key, or api token (it varies from service to service) and update the configuration of the Terminal, usually in its web.config. For more information, see "Configuring Terminals"). Importantly, you can use Terminals without running your own local version of them. You simply point your Hub at the Terminals operated by Fr8.co or some other operator. For more information on this, see "Incorporating Public Terminals into your Development Environment").

*Note*: each Terminal has a setting in its Web.config file that specifies the address of the terminal. It looks like this:
`  <add key="terminalName.TerminalEndpoint" value="http://localhost:8888" />`
We don't store port settings in the repository, so once you open the solution for the first time Visual Studio will assign random port numbers for all web applications.
If you are using IIS express you can either manually change the address in Web section of project properties for each terminal you are about to run. Or you can use Fr8Core\PortSetter utillity application, that will try to do it for you for all the terminals. 

3. Run Fr8. 

What you should see:

A web page should appear that has been produced by your HubWeb project. Curently this is on port 28672:
![](./hubwebpage.png)

For each *terminal* that's running, an additional web page will appear:
![](./startuppages.png)

(These pages serve no purpose except to signal that a Terminal is up and running.)

At this point, you should be able to register an account on your HubWeb project, and create Plans that use the Terminals you've started.

You may want to do [additional configuration](/Docs/ForDevelopers/DevelopmentGuides/Terminals/dotNet/ConfiguringHubAdvanced.md) to bring more Fr8 systems online.

Troubleshooting
----------------

If you get build errors and want help, post to one of the [.Net support resources](/Docs/ForDevelopers/SDK/.NET/HelpResources.md).

####If you're getting Prebuild.bat errors....

These are generally related to Visual Studio being unable to find npm.exe. [More info](/Docs/ForDevelopers/DevelopmentGuides/Terminals/dotNet/TerminalDeveloping-GettingStarted.md).

####If you're getting javascript errors....
You may need to run "bower install" in your project root directory to get the javascript libraries loaded in. You can run this command in the Package Manager Console or from the Windows Command Prompt while in the project source directory.


