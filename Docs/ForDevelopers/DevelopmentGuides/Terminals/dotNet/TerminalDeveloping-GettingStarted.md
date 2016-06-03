# Prelimetary work on the Azure deployment
[Back to Terminal Development on .Net](../DevGuide_DotNet.md)

####Required software:

1. [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx) is required. The Community Edition is known to work at the time of this writing. 
2. [Node.js](https://nodejs.org/en/)
3. [Git](https://git-scm.com/download/win): Once installed, Visual Studio will allow you to manage the git repository. If you prefer a more robust GUI for Git, consider SourceTree. Note that SourceTree is a client interacts with the git system installation and is not required for this project.
4. [TypeScript 1.5](http://blogs.msdn.com/b/typescript/archive/2015/07/20/announcing-typescript-1-5.aspx) (NOTE THAT 1.6 and later does NOT currently work!). Visual Studio 2015 will prompt you to upgrade TypeScript; close the dialog without updating TypeScript.
5. .NET 3.5 must be [enabled](http://windows.microsoft.com/en-us/windows/turn-windows-features-on-off#1TC=windows-7) in your environment.

####Database

If you are planning to run Hub locally you'll need to configure a database.
We use EntityFramework 6.1 to interact with data. We use CodeFirst Migrations.

######Creating a new local database
1. Check the name of the connection string in Web.config. This is probably "DockyardDB2"
2. Create a local database on your computer using either Sql Server or Sql Server Express. LocalDB is not recommended because it won't work with the project's default connection string.
Find Sql Server Express at http://www.hanselman.com/blog/DownloadSQLServerExpress.aspx. Make sure you choose the Default Instance and change the database ID to DockyardDB2,
Dont choose NamedInstance in the Installer (Thats what binds the default database to 1433, making it accessible as "." , which is what we have in our web.config)
3. Verify that you can connect to your local database by using the Server Explorer in Visual Studio. I like doing this because it separate local db issues from project/connection/EF issues. Until you can connect to your db from Server Explorer, you don't want to be messing with the project.
4. To fill your empty local database with tables and columns, you can  use Package Manager Console and run the Update-Database command. This is part of EF Migrations. If you're new to EF Migrations, stop here and go read about them. 
At the time of this writing, this ithe Update-Database command:
Update-Database -StartUpProjectName "HubWeb"  -ProjectName Data -Verbose
however, this not a required step, as it's done automatically when you try to run the server
5. Note that a successful Update-Database will also fill your database with some seed data. You can modify the seed data in the MigrationConfiguration folder. Feel free to add seed data. It isn't used on Production. 


####Configure ports

HubWeb's port is 30643. To learn what port a terminal has you can check it's Web.config file and find a key "terminalName.TerminalEndpoint". Make sure that you have set the same port number in project properties ( Web tab, Project URL ).

[Back to Terminal Development on .Net](../DevGuide_DotNet.md)
