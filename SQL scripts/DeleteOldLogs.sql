
--Deleting logs older then two weeks
DELETE FROM History  WHERE CreateDate < DATEADD(day, -14, GETDATE())
GO

DELETE FROM Containers  WHERE CreateDate < DATEADD(day, -14, GETDATE())
GO

DELETE FROM MtData  INNER JOIN [MtTypes] on [MtData].[Type] = [MtTypes].[id] Where [MtTypes].[ManifestId] = 13 and [MtData].[CreatedAt] < DATEADD(day, -14, GETDATE())
GO
