SELECT [Database], [ROWS] AS DataFileSizeMB, [LOG] AS LogFileSizeMB FROM

(
SELECT      sys.databases.name AS [Database],
            size*8/1024 AS [DataFileSizeMb], type_desc
			
FROM        sys.databases 
JOIN        sys.master_files
ON          sys.databases.database_id=sys.master_files.database_id
WHERE sys.databases.name = DB_NAME()
) src
PIVOT 
(
	MAX(DataFileSizeMb)
	FOR type_desc IN ([ROWS], [LOG])
) pvt