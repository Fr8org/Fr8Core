# Scheduling
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

*SchedulerController* currently has two functions setting delay and recurrence. First, delay the execution of the activities in the plan. The other one, is using the Hub to recur a process until terminal replies with status 200 OK. Terminal passes the interval in minutes and the Hub will send again request to the terminal iteratively.    
### *Execution Delay:* /sheduler/notify    
The delay between activity execution can are implemented through /schedule/notify.     
This endpoint expects the POST request:    
Name |	Type |	Nullable	| Default |	Description    
 --- | --- | --- | --- | ---    
ContainerId | 	GUID | 	no |	 - |	Specifies the container that needs to postpone the plan execution.
StartTime | 	DateTimeOffset | 	no | 	- |	The date and time when the execution of the remaining activities should continue.      
The request example to the endpoint /schedule/notify:    
```javascript
{
	"ContainerId" : "25cc7c40-c385-4f13-8b19-29457838cfe6",
	"StartTime" : "5/16/2016 6:43:20 AM +00:00"
}
```
Response example: 200 OK    

### *Recurrence:* /sheduler/poll    
Scheduler provides the ability to resend requests with specified data to the terminal until the latter responses with status 200 OK.    
It works as follows. A terminal calls this endpoint with specified data and sets the time intervals.    
The request has the following form:    
*/scheduler/poll?jobId={0}&fr8AccountId={1}&minutes={2}&terminalId={3}*   
So the Hub will iteratively call the terminal until the latter replies with status 200 OK. Each time it will make a POST request with the specified above data in the URL:    
 *terminalEndpoint/terminals/terminalName/polling?jobId={0}&fr8AccountId={1}&pollingInterval={2}*    
**Note:** It should be noted that the terminal is required to have /polling endpoint that excepts specified above, otherwise the exception will be thrown.    

 [Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 