[Back](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PlatformIdependentTerminalDeveloperGuide.md)


## Terminal Discovery

Hubs maintain a list of Terminals that they know about and periodically send GET /discover requests to them to get updated information on the Terminals and their Activities.

If you are using the ["Public Hub" Development Approach](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ChoosingADevelopmentApproach.md), you will register your Terminal and ask the Hub to make a /discover call to you. If you are using the "Local Hub" Development Approach, you'll generally trigger /discover calls to your Terminal each time you startup your local Hub.

So, in either case, your Terminal needs to stand ready to respond with information about itself.

### Handling the /discover Request

[Here's the API definition of a typical terminal](https://terminalfr8core.fr8.co/swagger/ui/index#!/Terminal/Terminal_Get). 

Here is an example of response your terminal should return in response to **/discover** request:
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
* All properties in the above JSON are mandatory.
* You should generate a GUID that will uniquely identify your terminal. This GUID should be returned as **Definition.Id** propery value. This GUID should never change unless and until you generate a new version of your Terminal (which will in most respects be treated as a completely different Terminal). GUID is represented by 32 hexadecimal digits separated by hyphens: 00000000-0000-0000-0000-000000000000.
* Asign **Definition.name** and **Definition.label** to anything you want. Note, that **Definition.label** is the text that is shown to users in fr8 UI in activity selection pane.
* **Definition.version**  Set it to "1" initially. 
* **Definiton.endpoint** The address of the termninal's publically accessible HTTP endpoint. 
* **Definition.authenticationType** defines what kind of authentication your terminal is going to use. We will not use authentication for now. Read about possible values here [link to the page describing terminal authentication]
 
Your /discover response informs the Hub about the Activities you're currently supporting. 

* For each activity you have to supply **name** and **label**. Name should be unique across your terminal. Label is shown to users in fr8 UI in the activity selection pane. 
* Activtiy **version** Set it to "1" at the beginning.</i>
* If your Terminal communicates with branded web service(s), set the webService information so the Client will display correctly.
* **iconPath** should be an absolute URL. 
* **NeedsAuthentication** flag should be set if this activity requires an authentication token. The Client checks for this before passing on /configure and /run requests, and instead displays the initial authentication UI to the user. 

Consult [Activity Templates specs](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/ActivityTemplates.md) for more information on possible properties value.

After you have correctly implemented response to **/discover** request you are ready to be discovered by the Hub. 
