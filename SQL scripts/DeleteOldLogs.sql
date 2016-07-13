
--Deleting logs older then two weeks
DELETE FROM History  WHERE CreateDate < DATEADD(day, -14, GETDATE())
GO

DELETE FROM Containers  WHERE CreateDate < DATEADD(day, -14, GETDATE())
GO

DELETE FROM MtData  WHERE CreateDate < DATEADD(day, -14, GETDATE())
GO