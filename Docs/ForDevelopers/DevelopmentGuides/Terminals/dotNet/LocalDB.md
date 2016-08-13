##Creating a new local database

1. Check the name of the connection string in Web.config. This should be "Fr8LocalDB".

2. Create a local database on your computer using either Sql Server or Sql Server Express. LocalDB is not recommended because it won't work with the project's default connection string.
Make sure you choose the Default Instance and change the database ID to Fr8LocalDB.
Dont choose "NamedInstance" in the Installer (By avoiding that, the system binds the default database to 1433, making it accessible as "." , which is what we have in our web.config)
3. Verify that you can connect to your local database by using the Server Explorer in Visual Studio. I like doing this because it separate local db issues from project/connection/EF issues. Until you can connect to your db from Server Explorer, you don't want to be messing with the project.
4. To fill your empty local database with tables and columns,  use Package Manager Console and run the Update-Database command. This is part of EF Migrations. If you're new to EF Migrations, stop here and go read about them.
This is the Update-Database command:
Update-Database -StartUpProjectName "HubWeb"  -ProjectName Data -Verbose
However, this not a required step, as it's done automatically when you try to run the server
Note that a successful Update-Database will also fill your database with some seed data.
