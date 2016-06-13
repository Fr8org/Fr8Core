SELECT 
	/* The first field is table name, all the next fields are table metrics */
    t.NAME AS TableName,
    p.[Rows],
    (sum(a.used_pages) * 8) / 1024 as UsedSpaceMB
FROM 
    sys.tables t
INNER JOIN      
    sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN 
    sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN 
    sys.allocation_units a ON p.partition_id = a.container_id
WHERE 
    t.NAME NOT LIKE 'dt%' AND
    i.OBJECT_ID > 255 AND   
    i.index_id <= 1 AND
	t.NAME NOT LIKE '_%Template' AND 
	t.NAME IN ('Actions', 'Containers', 'MtData', 'Plans', 'Job', 'JobQueue', 'History')
GROUP BY 
    t.NAME, i.object_id, i.index_id, i.name, p.[Rows]
ORDER BY 
    object_name(i.object_id) 