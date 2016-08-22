--select * from terminalregistration

DECLARE @EndpointsToDel TABLE 
(
	Port varchar(5)
)

INSERT INTO @EndpointsToDel (Port)
select Max(RIGHT ([Endpoint], CHARINDEX (':', REVERSE ([Endpoint]))-1)) As Port from TerminalRegistration 
group by (CASE when CHARINDEX (':', REVERSE ([Endpoint])) = 0
            then ''
        else 
            RIGHT ([Endpoint], CHARINDEX (':', REVERSE ([Endpoint])))end)
Having Count(*) > 1

DELETE TerminalRegistration FROM TerminalRegistration INNER JOIN @EndpointsToDel ET
ON RIGHT ([Endpoint], CHARINDEX (':', REVERSE ([Endpoint]))-1) = ET.Port  
WHERE CHARINDEX ('localhost:', [Endpoint]) > 0
