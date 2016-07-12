[Back](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PlatformIdependentTerminalDeveloperGuide.md)


## Terminal Discovery

For Fr8 the terminal is mainly a container with activities. The first thing the Hub will do when you register new terminal is sending HTTP GET request to it's **/discover** endpoint:

	http://terminal.com/discover
    
In the response to this request your terminal must return information about the terminal itself and acitivies this terminal is managing. Here is an example of response your terminal should return in response to **/discover** request:
```javascript
{
   "Definition":{
      "id":"{generate some GUID value here}",
      "name":"MyTerminal",
      "label":"My Teriminal",
      "version":"1",
      "endpoint":"http://terminal.com",
      "authenticationType":1
   },
   "Activities":[
      {
         "name":"My_fist_activity",
         "label":"My first activtiy",
         "version":"1",
         "webService":{
            "name":"My Terminal",
            "iconPath":"http://terminal.com/my-terminal-icon.png"
         },
         "Category":"Processors",
         "Type":"Standard",
         "minPaneWidth":330,
         "NeedsAuthentication":false,
      }
   ]
}
```

There are few important notes here:
* All properties it the above JSON are mandatory.
* You should generate some GUID that will uniquely identify the terminal. This GUID should be returned as **Definition.Id** propery value. This GUID should never change over the time. GUID is represented by 32 hexadecimal digits separated by hyphens: 00000000-0000-0000-0000-000000000000.
* Asign **Definition.name** and **Definition.label** to anything you want. Note, that **Definition.label** is the text that is shown to users in fr8 UI in activity selection pane.
* **Definition.version** is needed for your terminal evolution. Set it to "1" at the beginning. 
* **Definiton.endpoint** is very important property. This property should represent the address of the termninal's publically accessible HTTP endpoint. 
* **Definition.authenticationType** defines what kind of authentication your terminal is going to use. We will not use authentication for now. Read about possible values here [link to the page describing terminal authentication]
* For each activity you have to supply **name** and **label**. Name should be unique across your terminal. Label is shown to users in fr8 UI in activity selection pane. 
* Activtiy **version** is needed for your activity evolution. Set it to "1" at the beginning.</i>
* Don't forget to supply **webService** information.
* **iconPath** should be an absolute URL. 
* **NeedsAuthentication** flag shows if your particular activity wants to use authentication. 

Consult [Activity Templates specs](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/ActivityTemplates.md) for more information on possible properties value.

After you have correctly implemented response to **/discover** request you are ready to be discovered by the Hub. Let's register your terminal now!
