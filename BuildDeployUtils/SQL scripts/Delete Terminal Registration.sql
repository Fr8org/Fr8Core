USE [fr8Db1]
GO

SELECT [Id]
      ,[Name]
      ,[Version]
      ,[TerminalStatus]
      ,[Endpoint]
      ,[LastUpdated]
      ,[CreateDate]
      ,[UserDO_Id]
      ,[Description]
      ,[AuthenticationType]
      ,[PublicIdentifier]
      ,[Secret]
      ,[Label]
  FROM [dbo].[Terminals]
GO


delete from [ActivityTemplate] where Id = '735DA590-B3E5-4E5E-9C61-CFC60E3E2A1C'
delete from dbo.Terminals where Name = 'terminalExample';
delete from dbo.TerminalRegistration where Endpoint like '%terminalExample%'