# Building a .Net Terminal - Getting Started

[.NET SDK Home](https://github.com/Fr8org/Fr8Core/blob/FR-3375/Docs/ForDevelopers/SDK/.NET/Home.md)

[.NET Terminal Developer Guide Home](../DevGuide_DotNet.md)

####Required software:

1. Visual Studio 2015 is required. The Community Edition is known to work at the time of this writing. 
2. [Node.js](https://nodejs.org/en/) See notes below
4. TypeScript 1.8 (automatically installed by recent versions of Visual Studio)
5. .NET 4.5 is required. In addition, .NET 3.5 must also be enabled in your environment. Instructions are [here](http://windows.microsoft.com/en-us/windows/turn-windows-features-on-off#1TC=windows-7).
6. Git
7. [Azure SDK 2.8.#](http://www.microsoft.com/en-us/download/details.aspx?id=48178)



Getting Node to work with Visual Studio has proven to be tricky. Simply installing Node onto your computer isn't usually sufficient. The test is this: if you can go to the Package Manager Console and type npm, then Visual Studio can see your local node installation. If it can't, the key seems to be to manually add a path to your node installation to your system PATH. 

Once Node.js and git are installed, verify their installations by executing their commands in the Package Manager Console (i.e. type "npm" and "git"). If you installed Node.js and git while Visual Studio is running, you will have to restart Visual Studio. Also, if you get errors while running git commands in the Package Manager Console, you will need to enter your credentials in either Visual Studio or from the Windows Command Prompt.

Knowledge of git and git branching is fundamental to success in this project. Read the sections at Our Git Environment. If you are not experienced with git, It's highly recommended that you work through the first 3 chapters of the Git Book. A very good description of the pattern that we follow can be found here.




Azure SDK 2.8. Without this SDK you may get an error while opening fr8 solution in Visual Studio.

####Database

If you are planning to run Hub locally you'll need to configure a sql database.
Fr8 uses EntityFramework 6.1 to interact with data. Fr8 uses CodeFirst Migrations.

######Creating a new local database
1. Check the name of the connection string in Web.config. This is probably "DockyardDB2"
2. Create a local database on your computer using either Sql Server or Sql Server Express. LocalDB is not recommended because it won't work with the project's default connection string. Make sure you choose the Default Instance and change the database ID to DockyardDB2.
Dont choose NamedInstance in the Installer (Thats what binds the default database to 1433, making it accessible as "." , which is what we have in our web.config)
3. Verify that you can connect to your local database by using the Server Explorer in Visual Studio. I like doing this because it separate local db issues from project/connection/EF issues. Until you can connect to your db from Server Explorer, you don't want to be messing with the project.
4. To fill your empty local database with tables and columns, you can use Package Manager Console and run the Update-Database command. This is part of EF Migrations. If you're new to EF Migrations, stop here and go read about them. 
This is the Update-Database command:
Update-Database -StartUpProjectName "HubWeb"  -ProjectName Data -Verbose
However, this not a required step, as it's done automatically when you try to run the server
5. Note that a successful Update-Database will also fill your database with some seed data. 


####Configure ports

HubWeb's port is 30643. To learn what port a terminal has you can check it's Web.config file and find a key "terminalName.TerminalEndpoint". Make sure that you have set the same port number in project properties ( Web tab, Project URL ).


