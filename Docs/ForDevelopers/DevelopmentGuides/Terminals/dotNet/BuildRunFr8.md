Building and Running Fr8
========================


1. Open the Fr8 solution in Visual Studio.

2. Decide which Terminals you want to run with and verify that your solution is set to open [Multiple Startup Projects](/Docs/ForDevelopers/DevelopmentGuides/Terminals/dotNet/MultipleStartupProjects.md)

3. Run Fr8. If you get build errors and want help, post to one of the [.Net support resources](/Docs/ForDevelopers/SDK/.NET/HelpResources.md).

What you should see:

A web page should appear that has been produced by your HubWeb project. Curently this is on port 28672:
![](./dotNet/hubwebpage.png)

For each *terminal* that's running, an additional web page will appear:
![](./dotNet/startuppages.png)

(These pages serve no purpose except to signal that a Terminal is up and running.)

If you see error messages related to "Prebuild.bat", be sure to add npm and git to your PATH environment. See Are you getting the "Prebuild.bat exits with code 1" error?


You may need to run "bower install" in your project root directory to get the javascript libraries loaded in. You can run this command in the Package Manager Console or from the Windows Command Prompt while in the project source directory.
