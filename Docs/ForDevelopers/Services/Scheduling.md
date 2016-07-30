# Scheduling
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

*AlarmsController* currently has two functions setting delay and recurrence. First, delay the execution of the activities in the plan. The other one, is using the Hub to recur a process until terminal replies with status 200 OK. Terminal passes the interval in minutes and the Hub will send again request to the terminal iteratively.    
### *Execution Delay:* /alarms    
The delay between activity execution can are implemented through /alarms.     
This endpoint expects the POST request:    
Name |	Type |	Nullable	| Default |	Description    
 --- | --- | --- | --- | ---    
ContainerId | 	GUID | 	no |	 - |	Specifies the container that needs to postpone the plan execution.
StartTime | 	DateTimeOffset | 	no | 	- |	The date and time when the execution of the remaining activities should continue.      
The request example to the endpoint /alarms:    
```javascript
{
	"ContainerId" : "25cc7c40-c385-4f13-8b19-29457838cfe6",
	"StartTime" : "5/16/2016 6:43:20 AM +00:00"
}
```
Response example: 200 OK    

### *Recurrence:* /alarms/polling    
Alarms provide the ability to resend requests with specified data to the terminal until the latter responses with status 200 OK.    
It works as follows. A terminal calls this endpoint with specified data and sets the time intervals.    

Name |	Type |	Nullable	| Default |	Description   
--- | --- | --- | --- | ---  
job_id |	string |	 no	| -	| This serves as a job identifier inside the Terminal and Hub. This parameter can be an external account id for terminal, while it will serve as a id for the job to be executed using Hangfire
fr8_account_id |	string |	no |	- |	Fr8 Account Identifier used only inside a terminal.
minutes |	string |	no |	- |	Number of minutes in the form of 15, 10, 5 that specify the delay between iterations.
terminal_id |	string |	no |	-	| Terminal Identifier to help Hub understand which terminal to reply to.    

The request has the following form:    
*/alarms/polling?job_id={0}&fr8_account_id={1}&minutes={2}&terminal_id={3}*    
So the Hub will iteratively call the terminal until the latter replies with status 200 OK. Each time it will make a POST request with the specified above data in the URL:    
 *[terminalEndpoint]/terminals/[terminalName]/polling?job_id={0}&fr8_account_id={1}&polling_interval={2}*   
**Note:** It should be noted that the terminal is required to have /polling endpoint that excepts specified above, otherwise the exception will be thrown.    

 [Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
